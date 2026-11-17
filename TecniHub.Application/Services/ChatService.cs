using System.Collections.Concurrent;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Chat;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;

namespace TecniHub.Application.Services;

public sealed class ChatService : IChatService
{
    private readonly IPublicacionServicioRepository _servicioRepository;
    private readonly IPublicacionPiezaRepository _piezaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ITecnicoRepository _tecnicoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly INotificacionPushService? _notificacionService;

    // Almacén en memoria concurrente y persistente durante el ciclo de vida del backend
    private static readonly ConcurrentDictionary<string, ConversacionModel> Conversaciones = new();
    private static readonly ConcurrentDictionary<string, List<MensajeModel>> MensajesPorConversacion = new();
    private static long _nextMensajeId = 1;

    private sealed record ConversacionModel(
        string Id,
        string TipoContexto, // "SERVICIO" o "PIEZA"
        int ContextoId,
        int Participante1UsuarioId,
        string Participante1Nombre,
        string Participante1Rol,
        int Participante2UsuarioId,
        string Participante2Nombre,
        string Participante2Rol,
        string TituloContexto,
        string EstadoContexto,
        DateTime CreatedAt,
        DateTime UltimaActividad,
        string? UltimoMensaje
    );

    private sealed record MensajeModel(
        long Id,
        string ConversacionId,
        int EmisorId,
        string EmisorNombre,
        string EmisorRol,
        string Contenido,
        DateTime EnviadoAt
    );

    public ChatService(
        IPublicacionServicioRepository servicioRepository,
        IPublicacionPiezaRepository piezaRepository,
        IClienteRepository clienteRepository,
        ITecnicoRepository tecnicoRepository,
        IUsuarioRepository usuarioRepository,
        INotificacionPushService? notificacionService = null)
    {
        _servicioRepository = servicioRepository;
        _piezaRepository = piezaRepository;
        _clienteRepository = clienteRepository;
        _tecnicoRepository = tecnicoRepository;
        _usuarioRepository = usuarioRepository;
        _notificacionService = notificacionService;
    }

    public async Task<ConversacionResumenDto> ObtenerOIniciarConversacionServicioAsync(
        int servicioId,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        var servicio = await _servicioRepository.ObtenerPorIdAsync(servicioId, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la publicación de servicio con ID {servicioId}.");

        // Regla de Negocio HU-011: Condición de apertura
        // "La sala de chat entre dos usuarios solo puede habilitarse si existe una interacción previa validada por el sistema (ej. un servicio aceptado)."
        var estado = servicio.Estado?.Trim() ?? string.Empty;
        var esValido = string.Equals(estado, "EnProceso", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(estado, "Aceptado", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(estado, "Completado", StringComparison.OrdinalIgnoreCase);

        if (!esValido || !servicio.TecnicoId.HasValue)
        {
            throw new InvalidOperationException("La sala de chat solo puede habilitarse una vez que el servicio ha sido aceptado por un técnico.");
        }

        var clienteUsuarioId = servicio.Cliente?.UsuarioId ?? servicio.ClienteId;
        var clienteNombre = servicio.Cliente != null
            ? $"{servicio.Cliente.Nombre} {servicio.Cliente.Apellidos}".Trim()
            : $"Cliente #{servicio.ClienteId}";

        var tecnicoUsuarioId = servicio.Tecnico?.UsuarioId ?? servicio.TecnicoId.Value;
        var tecnicoNombre = servicio.Tecnico?.NombreCompleto ?? $"Técnico #{servicio.TecnicoId}";

        // Regla de Negocio HU-011: Privacidad y seguridad
        // "Ningún otro usuario (ni cliente ni técnico ajeno al servicio) puede leer los mensajes de esa sala de chat específica."
        if (usuarioIdAutenticado.HasValue)
        {
            var esParticipante = usuarioIdAutenticado.Value == clienteUsuarioId ||
                                 usuarioIdAutenticado.Value == tecnicoUsuarioId;
            if (!esParticipante)
            {
                throw new UnauthorizedAccessException("No tienes autorización para acceder a esta conversación privada ajena a tu servicio.");
            }
        }

        var convId = $"srv-{servicioId}";
        var conv = Conversaciones.GetOrAdd(convId, _ => new ConversacionModel(
            Id: convId,
            TipoContexto: "SERVICIO",
            ContextoId: servicioId,
            Participante1UsuarioId: clienteUsuarioId,
            Participante1Nombre: clienteNombre,
            Participante1Rol: "CLIENTE",
            Participante2UsuarioId: tecnicoUsuarioId,
            Participante2Nombre: tecnicoNombre,
            Participante2Rol: "TECNICO",
            TituloContexto: servicio.Titulo,
            EstadoContexto: servicio.Estado ?? "Pendiente",
            CreatedAt: DateTime.UtcNow,
            UltimaActividad: DateTime.UtcNow,
            UltimoMensaje: null
        ));

        MensajesPorConversacion.TryAdd(convId, new List<MensajeModel>());

        var mensajes = MensajesPorConversacion[convId];
        lock (mensajes)
        {
            return MapearAResumen(conv, mensajes.Count);
        }
    }

    public async Task<IReadOnlyList<MensajeDto>> ObtenerHistorialMensajesServicioAsync(
        int servicioId,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        await ObtenerOIniciarConversacionServicioAsync(servicioId, usuarioIdAutenticado, cancellationToken);

        var convId = $"srv-{servicioId}";
        if (!MensajesPorConversacion.TryGetValue(convId, out var mensajes))
        {
            return Array.Empty<MensajeDto>();
        }

        lock (mensajes)
        {
            return mensajes
                .OrderBy(m => m.EnviadoAt)
                .Select(m => MapearAMensajeDto(m, usuarioIdAutenticado))
                .ToList();
        }
    }

    public async Task<MensajeDto> EnviarMensajeServicioAsync(
        int servicioId,
        EnviarMensajeRequestDto request,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Contenido))
            throw new ReglaNegocioInvalidaException("El mensaje no puede estar vacío.");

        if (request.Contenido.Length > 2000)
            throw new ReglaNegocioInvalidaException("El mensaje no puede exceder los 2000 caracteres.");

        var convResumen = await ObtenerOIniciarConversacionServicioAsync(servicioId, usuarioIdAutenticado, cancellationToken);
        var convId = $"srv-{servicioId}";

        var emisorId = usuarioIdAutenticado ?? request.RemitenteUsuarioId ?? convResumen.ClienteUsuarioId;
        var (emisorNombre, emisorRol) = await ResolverDatosEmisorAsync(emisorId, convResumen, cancellationToken);

        var mensajeId = Interlocked.Increment(ref _nextMensajeId);
        var ahora = DateTime.UtcNow;

        var nuevoMensaje = new MensajeModel(
            Id: mensajeId,
            ConversacionId: convId,
            EmisorId: emisorId,
            EmisorNombre: emisorNombre,
            EmisorRol: emisorRol,
            Contenido: request.Contenido.Trim(),
            EnviadoAt: ahora
        );

        var lista = MensajesPorConversacion.GetOrAdd(convId, _ => new List<MensajeModel>());
        lock (lista)
        {
            lista.Add(nuevoMensaje);
        }

        // Actualizar resumen de la conversación
        if (Conversaciones.TryGetValue(convId, out var c))
        {
            Conversaciones[convId] = c with
            {
                UltimaActividad = ahora,
                UltimoMensaje = nuevoMensaje.Contenido
            };

            if (_notificacionService != null)
            {
                var destinatarioId = c.Participante1UsuarioId == emisorId ? c.Participante2UsuarioId : c.Participante1UsuarioId;
                if (destinatarioId > 0)
                {
                    await _notificacionService.NotificarNuevoMensajeChatAsync(
                        destinatarioId,
                        emisorNombre,
                        request.Contenido.Trim(),
                        c.ContextoId,
                        c.TipoContexto == "PIEZA" ? "Pieza" : "Servicio",
                        cancellationToken);
                }
            }
        }

        return MapearAMensajeDto(nuevoMensaje, usuarioIdAutenticado);
    }

    public async Task<ConversacionResumenDto> ObtenerOIniciarConversacionPiezaAsync(
        int piezaId,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        var pieza = await _piezaRepository.ObtenerPorIdAsync(piezaId, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la publicación de refacción con ID {piezaId}.");

        var tecnicoUsuarioId = pieza.Tecnico?.UsuarioId ?? pieza.TecnicoId;
        var tecnicoNombre = pieza.Tecnico?.NombreCompleto ?? $"Técnico #{pieza.TecnicoId}";

        var interesadoId = usuarioIdAutenticado ?? 0;
        var interesadoNombre = "Usuario Interesado";
        if (interesadoId > 0)
        {
            var user = await _usuarioRepository.ObtenerPorIdAsync(interesadoId, cancellationToken);
            if (user != null) interesadoNombre = user.Correo;
        }

        var convId = $"pie-{piezaId}-{interesadoId}";
        var conv = Conversaciones.GetOrAdd(convId, _ => new ConversacionModel(
            Id: convId,
            TipoContexto: "PIEZA",
            ContextoId: piezaId,
            Participante1UsuarioId: interesadoId,
            Participante1Nombre: interesadoNombre,
            Participante1Rol: "INTERESADO",
            Participante2UsuarioId: tecnicoUsuarioId,
            Participante2Nombre: tecnicoNombre,
            Participante2Rol: "TECNICO",
            TituloContexto: pieza.NombrePieza,
            EstadoContexto: pieza.EstadoDisponibilidad,
            CreatedAt: DateTime.UtcNow,
            UltimaActividad: DateTime.UtcNow,
            UltimoMensaje: null
        ));

        MensajesPorConversacion.TryAdd(convId, new List<MensajeModel>());

        var mensajes = MensajesPorConversacion[convId];
        lock (mensajes)
        {
            return MapearAResumen(conv, mensajes.Count);
        }
    }

    public async Task<IReadOnlyList<MensajeDto>> ObtenerHistorialMensajesPiezaAsync(
        int piezaId,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        var conv = await ObtenerOIniciarConversacionPiezaAsync(piezaId, usuarioIdAutenticado, cancellationToken);
        if (!MensajesPorConversacion.TryGetValue(conv.Id, out var mensajes))
        {
            return Array.Empty<MensajeDto>();
        }

        lock (mensajes)
        {
            return mensajes
                .OrderBy(m => m.EnviadoAt)
                .Select(m => MapearAMensajeDto(m, usuarioIdAutenticado))
                .ToList();
        }
    }

    public async Task<MensajeDto> EnviarMensajePiezaAsync(
        int piezaId,
        EnviarMensajeRequestDto request,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Contenido))
            throw new ReglaNegocioInvalidaException("El mensaje no puede estar vacío.");

        var convResumen = await ObtenerOIniciarConversacionPiezaAsync(piezaId, usuarioIdAutenticado, cancellationToken);
        var convId = convResumen.Id;

        var emisorId = usuarioIdAutenticado ?? request.RemitenteUsuarioId ?? convResumen.ClienteUsuarioId;
        var (emisorNombre, emisorRol) = await ResolverDatosEmisorAsync(emisorId, convResumen, cancellationToken);

        var mensajeId = Interlocked.Increment(ref _nextMensajeId);
        var ahora = DateTime.UtcNow;

        var nuevoMensaje = new MensajeModel(
            Id: mensajeId,
            ConversacionId: convId,
            EmisorId: emisorId,
            EmisorNombre: emisorNombre,
            EmisorRol: emisorRol,
            Contenido: request.Contenido.Trim(),
            EnviadoAt: ahora
        );

        var lista = MensajesPorConversacion.GetOrAdd(convId, _ => new List<MensajeModel>());
        lock (lista)
        {
            lista.Add(nuevoMensaje);
        }

        if (Conversaciones.TryGetValue(convId, out var c))
        {
            Conversaciones[convId] = c with
            {
                UltimaActividad = ahora,
                UltimoMensaje = nuevoMensaje.Contenido
            };
        }

        return MapearAMensajeDto(nuevoMensaje, usuarioIdAutenticado);
    }

    public Task<IReadOnlyList<ConversacionResumenDto>> ObtenerMisConversacionesAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var resultado = new List<ConversacionResumenDto>();

        foreach (var conv in Conversaciones.Values)
        {
            if (conv.Participante1UsuarioId == usuarioId || conv.Participante2UsuarioId == usuarioId)
            {
                var total = MensajesPorConversacion.TryGetValue(conv.Id, out var list) ? list.Count : 0;
                resultado.Add(MapearAResumen(conv, total));
            }
        }

        return Task.FromResult<IReadOnlyList<ConversacionResumenDto>>(
            resultado.OrderByDescending(c => c.UltimaActividad).ToList()
        );
    }

    private async Task<(string Nombre, string Rol)> ResolverDatosEmisorAsync(
        int emisorId,
        ConversacionResumenDto conv,
        CancellationToken cancellationToken)
    {
        if (emisorId == conv.ClienteUsuarioId)
        {
            return (conv.ClienteNombre, "CLIENTE");
        }

        if (emisorId == conv.TecnicoUsuarioId)
        {
            return (conv.TecnicoNombre, "TECNICO");
        }

        var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(emisorId, cancellationToken);
        if (tecnico != null) return (tecnico.NombreCompleto, "TECNICO");

        var cliente = await _clienteRepository.ObtenerPorUsuarioIdAsync(emisorId, cancellationToken);
        if (cliente != null) return ($"{cliente.Nombre} {cliente.Apellidos}".Trim(), "CLIENTE");

        var user = await _usuarioRepository.ObtenerPorIdAsync(emisorId, cancellationToken);
        if (user != null) return (user.Correo, user.Rol);

        return ($"Usuario #{emisorId}", "USUARIO");
    }

    private static ConversacionResumenDto MapearAResumen(ConversacionModel c, int totalMensajes) =>
        new(
            Id: c.Id,
            TipoContexto: c.TipoContexto,
            ContextoId: c.ContextoId,
            TituloContexto: c.TituloContexto,
            EstadoContexto: c.EstadoContexto,
            ClienteUsuarioId: c.Participante1UsuarioId,
            ClienteNombre: c.Participante1Nombre,
            TecnicoUsuarioId: c.Participante2UsuarioId,
            TecnicoNombre: c.Participante2Nombre,
            UltimaActividad: c.UltimaActividad,
            UltimoMensaje: c.UltimoMensaje,
            TotalMensajes: totalMensajes
        );

    private static MensajeDto MapearAMensajeDto(MensajeModel m, int? usuarioActualId)
    {
        var localTime = m.EnviadoAt.ToLocalTime();
        var hora = localTime.ToString("hh:mm tt");
        var esHoy = localTime.Date == DateTime.Today;
        var horaFormateada = esHoy ? $"Hoy, {hora}" : $"{localTime:dd/MM/yyyy}, {hora}";

        return new MensajeDto(
            Id: m.Id,
            ConversacionId: m.ConversacionId,
            EmisorId: m.EmisorId,
            EmisorNombre: m.EmisorNombre,
            EmisorRol: m.EmisorRol,
            Contenido: m.Contenido,
            EnviadoAt: m.EnviadoAt,
            HoraFormateada: horaFormateada,
            EsMio: usuarioActualId.HasValue && usuarioActualId.Value == m.EmisorId
        );
    }
}
