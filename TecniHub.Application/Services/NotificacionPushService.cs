using System.Collections.Concurrent;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Notificaciones;
using TecniHub.Domain.Entities;

namespace TecniHub.Application.Services;

public sealed class NotificacionPushService : INotificacionPushService
{
    private readonly IPublicacionServicioRepository _servicioRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ITecnicoRepository _tecnicoRepository;

    private static readonly ConcurrentDictionary<int, List<NotificacionDto>> NotificacionesPorUsuario = new();
    private static readonly ConcurrentDictionary<string, SuscripcionPushModel> Suscripciones = new();
    private static long _nextNotificacionId = 1;

    private sealed record SuscripcionPushModel(
        string Endpoint,
        string? P256dh,
        string? Auth,
        string? Plataforma,
        string? DispositivoToken,
        int? UsuarioId,
        DateTime FechaRegistro
    );

    public NotificacionPushService(
        IPublicacionServicioRepository servicioRepository,
        IClienteRepository clienteRepository,
        ITecnicoRepository tecnicoRepository)
    {
        _servicioRepository = servicioRepository;
        _clienteRepository = clienteRepository;
        _tecnicoRepository = tecnicoRepository;
    }

    public Task<bool> SuscribirAsync(
        SuscripcionPushRequestDto request,
        int? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Endpoint))
            throw new ArgumentException("El endpoint o token de suscripción push es requerido.");

        var model = new SuscripcionPushModel(
            Endpoint: request.Endpoint.Trim(),
            P256dh: request.P256dh?.Trim(),
            Auth: request.Auth?.Trim(),
            Plataforma: request.Plataforma?.Trim() ?? "Web",
            DispositivoToken: request.DispositivoToken?.Trim(),
            UsuarioId: usuarioId ?? request.UsuarioId,
            FechaRegistro: DateTime.UtcNow
        );

        Suscripciones[model.Endpoint] = model;
        return Task.FromResult(true);
    }

    public Task<bool> DesuscribirAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(endpoint)) return Task.FromResult(false);
        var removido = Suscripciones.TryRemove(endpoint.Trim(), out _);
        return Task.FromResult(removido);
    }

    public Task<IReadOnlyList<NotificacionDto>> ObtenerNotificacionesUsuarioAsync(
        int usuarioId,
        bool? soloNoLeidas = null,
        CancellationToken cancellationToken = default)
    {
        if (!NotificacionesPorUsuario.TryGetValue(usuarioId, out var lista))
        {
            return Task.FromResult<IReadOnlyList<NotificacionDto>>(Array.Empty<NotificacionDto>());
        }

        lock (lista)
        {
            var query = lista.AsEnumerable();
            if (soloNoLeidas.HasValue && soloNoLeidas.Value)
            {
                query = query.Where(n => !n.Leida);
            }

            var resultado = query.OrderByDescending(n => n.Fecha).ToList();
            return Task.FromResult<IReadOnlyList<NotificacionDto>>(resultado);
        }
    }

    public Task<bool> MarcarComoLeidaAsync(
        string notificacionId,
        int? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(notificacionId)) return Task.FromResult(false);

        if (usuarioId.HasValue && NotificacionesPorUsuario.TryGetValue(usuarioId.Value, out var listaUsuario))
        {
            lock (listaUsuario)
            {
                var index = listaUsuario.FindIndex(n => n.Id == notificacionId);
                if (index >= 0)
                {
                    listaUsuario[index] = listaUsuario[index] with { Leida = true };
                    return Task.FromResult(true);
                }
            }
        }

        // Buscar en todas las listas si no se proporcionó usuarioId
        foreach (var lista in NotificacionesPorUsuario.Values)
        {
            lock (lista)
            {
                var index = lista.FindIndex(n => n.Id == notificacionId);
                if (index >= 0)
                {
                    lista[index] = lista[index] with { Leida = true };
                    return Task.FromResult(true);
                }
            }
        }

        return Task.FromResult(false);
    }

    public Task<NotificacionDto> EnviarNotificacionAsync(
        EnviarNotificacionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var id = Interlocked.Increment(ref _nextNotificacionId).ToString();
        var ahora = DateTime.UtcNow;

        var colorInfo = EvaluarColorNotificacion(request.Tipo, request.CodigoColor, request.ColorHex);

        var notificacion = new NotificacionDto(
            Id: id,
            UsuarioId: request.UsuarioId,
            Titulo: request.Titulo.Trim(),
            Mensaje: request.Mensaje.Trim(),
            Tipo: request.Tipo.Trim(),
            CodigoColor: colorInfo.codigoColor,
            ColorHex: colorInfo.colorHex,
            ReferenciaId: request.ReferenciaId,
            TipoReferencia: request.TipoReferencia ?? "Servicio",
            Fecha: ahora,
            FechaRelativa: "Hace un momento",
            Leida: false
        );

        var lista = NotificacionesPorUsuario.GetOrAdd(request.UsuarioId, _ => new List<NotificacionDto>());
        lock (lista)
        {
            lista.Add(notificacion);
        }

        return Task.FromResult(notificacion);
    }

    public async Task<NotificacionDto> SimularEventoPushAsync(
        SimulacionEventoPushRequestDto request,
        int? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var destinoUsuarioId = usuarioId ?? 1;
        string titulo;
        string mensaje;
        string tipo = request.TipoEvento?.Trim() ?? "General";
        string codigoColor;
        string colorHex;
        string tipoReferencia = "Servicio";
        int? refId = request.ServicioId;

        switch (request.TipoEvento?.Trim().ToUpperInvariant())
        {
            case "TECNICOACEPTOSERVICIO":
            case "SERVICIOACEPTADO":
                titulo = "Servicio Aceptado";
                var tec = string.IsNullOrWhiteSpace(request.NombreContraparte) ? "@TechMaster99" : request.NombreContraparte;
                var srv = string.IsNullOrWhiteSpace(request.TituloItem) ? "reparación de consola" : request.TituloItem;
                mensaje = $"El técnico {tec} ha aceptado tu {srv}.";
                tipo = "ServicioAceptado";
                codigoColor = "Verde";
                colorHex = "#22C55E";
                break;

            case "CLIENTEFINALIZOSERVICIO":
            case "TRABAJOCOMPLETADO":
                titulo = "Trabajo Completado";
                var cli = string.IsNullOrWhiteSpace(request.NombreContraparte) ? "Rodrigo D." : request.NombreContraparte;
                mensaje = $"{cli} ha marcado el servicio como completado y te dejó una reseña.";
                tipo = "TrabajoCompletado";
                codigoColor = "Azul";
                colorHex = "#3B82F6";
                break;

            case "NUEVOMENSAJECHAT":
            case "NUEVOMENSAJE":
                titulo = "Nuevo Mensaje";
                var emisor = string.IsNullOrWhiteSpace(request.NombreContraparte) ? "FixIt_Juan" : request.NombreContraparte;
                var tema = string.IsNullOrWhiteSpace(request.TituloItem) ? "pieza 'Memoria RAM'" : request.TituloItem;
                mensaje = $"Tienes un nuevo mensaje de {emisor} sobre {tema}.";
                tipo = "NuevoMensaje";
                codigoColor = "Amarillo";
                colorHex = "#EAB308";
                tipoReferencia = "Chat";
                refId = request.PiezaId ?? request.ServicioId;
                break;

            default:
                titulo = "Notificación de Estado TecniHub";
                mensaje = $"Actualización en la plataforma: {request.TipoEvento}";
                codigoColor = "Azul";
                colorHex = "#3B82F6";
                break;
        }

        return await EnviarNotificacionAsync(new EnviarNotificacionRequestDto(
            UsuarioId: destinoUsuarioId,
            Titulo: titulo,
            Mensaje: mensaje,
            Tipo: tipo,
            CodigoColor: codigoColor,
            ColorHex: colorHex,
            ReferenciaId: refId,
            TipoReferencia: tipoReferencia
        ), cancellationToken);
    }

    public async Task NotificarCambioEstadoServicioAsync(
        int servicioId,
        string nuevoEstado,
        int usuarioAccionId,
        CancellationToken cancellationToken = default)
    {
        var servicio = await _servicioRepository.ObtenerPorIdAsync(servicioId, cancellationToken);
        if (servicio is null) return;

        var cliente = servicio.Cliente ?? await _clienteRepository.ObtenerPorUsuarioIdAsync(servicio.ClienteId, cancellationToken);
        var clienteUsuarioId = cliente?.UsuarioId ?? servicio.ClienteId;

        int? tecnicoUsuarioId = null;
        string nombreTecnico = "@Técnico";
        if (servicio.TecnicoId.HasValue)
        {
            var tecnico = servicio.Tecnico ?? await _tecnicoRepository.ObtenerPorUsuarioIdAsync(servicio.TecnicoId.Value, cancellationToken);
            if (tecnico is not null)
            {
                tecnicoUsuarioId = tecnico.UsuarioId;
                nombreTecnico = string.IsNullOrWhiteSpace(tecnico.NombreCompleto) ? $"@Tecnico_{tecnico.Id}" : $"@{tecnico.NombreCompleto}";
            }
        }

        var nombreCliente = cliente is not null
            ? $"{cliente.Nombre} {cliente.Apellidos.FirstOrDefault()}."
            : "Cliente";

        // Regla de Negocio HU-015: Enviar únicamente a los directamente involucrados
        if (string.Equals(nuevoEstado, "EnProceso", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(nuevoEstado, "Aceptado", StringComparison.OrdinalIgnoreCase))
        {
            // Notificar al Cliente: Servicio Aceptado
            await EnviarNotificacionAsync(new EnviarNotificacionRequestDto(
                UsuarioId: clienteUsuarioId,
                Titulo: "Servicio Aceptado",
                Mensaje: $"El técnico {nombreTecnico} ha aceptado tu {servicio.Titulo}.",
                Tipo: "ServicioAceptado",
                CodigoColor: "Verde",
                ColorHex: "#22C55E",
                ReferenciaId: servicio.Id,
                TipoReferencia: "Servicio"
            ), cancellationToken);
        }
        else if (string.Equals(nuevoEstado, "PendienteCierre", StringComparison.OrdinalIgnoreCase))
        {
            // Notificar al Cliente: Solicitud de Cierre
            await EnviarNotificacionAsync(new EnviarNotificacionRequestDto(
                UsuarioId: clienteUsuarioId,
                Titulo: "Solicitud de Cierre de Servicio",
                Mensaje: $"El técnico {nombreTecnico} ha finalizado el trabajo de tu servicio y solicita que confirmes la finalización.",
                Tipo: "SolicitudCierre",
                CodigoColor: "Amarillo",
                ColorHex: "#EAB308",
                ReferenciaId: servicio.Id,
                TipoReferencia: "Servicio"
            ), cancellationToken);
        }
        else if (string.Equals(nuevoEstado, "Completado", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(nuevoEstado, "Finalizado", StringComparison.OrdinalIgnoreCase))
        {
            // Notificar al Técnico: Trabajo Completado
            if (tecnicoUsuarioId.HasValue)
            {
                await EnviarNotificacionAsync(new EnviarNotificacionRequestDto(
                    UsuarioId: tecnicoUsuarioId.Value,
                    Titulo: "Trabajo Completado",
                    Mensaje: $"{nombreCliente} ha marcado el servicio como completado y te dejó una reseña.",
                    Tipo: "TrabajoCompletado",
                    CodigoColor: "Azul",
                    ColorHex: "#3B82F6",
                    ReferenciaId: servicio.Id,
                    TipoReferencia: "Servicio"
                ), cancellationToken);
            }
        }
    }

    public async Task NotificarNuevoMensajeChatAsync(
        int usuarioDestinoId,
        string emisorNombre,
        string contenido,
        int? referenciaId,
        string tipoReferencia = "Chat",
        CancellationToken cancellationToken = default)
    {
        var preview = contenido.Length > 50 ? string.Concat(contenido.AsSpan(0, 47), "...") : contenido;
        await EnviarNotificacionAsync(new EnviarNotificacionRequestDto(
            UsuarioId: usuarioDestinoId,
            Titulo: "Nuevo Mensaje",
            Mensaje: $"Tienes un nuevo mensaje de {emisorNombre}: \"{preview}\"",
            Tipo: "NuevoMensaje",
            CodigoColor: "Amarillo",
            ColorHex: "#EAB308",
            ReferenciaId: referenciaId,
            TipoReferencia: tipoReferencia
        ), cancellationToken);
    }

    public Task<EstadoConectividadDto> VerificarConectividadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new EstadoConectividadDto(
            Online: true,
            Timestamp: DateTime.UtcNow,
            Mensaje: "Conexión activa y estable con el servidor TecniHub.",
            Servidor: "TecniHub-API-v1",
            Version: "1.0.0"
        ));
    }

    private static (string codigoColor, string colorHex) EvaluarColorNotificacion(string tipo, string? codigo, string? hex)
    {
        if (!string.IsNullOrWhiteSpace(codigo) && !string.IsNullOrWhiteSpace(hex))
            return (codigo, hex);

        return tipo.Trim().ToUpperInvariant() switch
        {
            "SERVICIOACEPTADO" => ("Verde", "#22C55E"),
            "TRABAJOCOMPLETADO" => ("Azul", "#3B82F6"),
            "NUEVOMENSAJE" or "SOLICITUDCIERRE" => ("Amarillo", "#EAB308"),
            "ALERTA" or "DESCONEXION" => ("Rojo", "#EF4444"),
            _ => ("Azul", "#3B82F6")
        };
    }
}
