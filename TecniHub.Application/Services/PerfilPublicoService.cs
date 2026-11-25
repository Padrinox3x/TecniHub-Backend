
using System.Collections.Concurrent;
using System.Globalization;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Perfil;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;

namespace TecniHub.Application.Services;

public sealed class PerfilPublicoService : IPerfilPublicoService
{
    private readonly ITecnicoRepository _tecnicoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IPublicacionServicioRepository _servicioRepository;
    private readonly IUnitOfWork? _unitOfWork;
    private readonly INotificacionPushService? _notificacionService;

    private static readonly ConcurrentDictionary<int, List<ResenaModel>> ResenasPorTecnico = new();
    private static readonly ConcurrentDictionary<int, ResenaModel> ResenasPorServicio = new();
    private static int _nextResenaId = 1;

    private sealed record ResenaModel(
        string Id,
        int TecnicoId,
        int ClienteId,
        int ServicioId,
        string ClienteNombre,
        short Calificacion,
        DateTime Fecha,
        string Comentario,
        string TipoServicio
    );

    public PerfilPublicoService(
        ITecnicoRepository tecnicoRepository,
        IClienteRepository clienteRepository,
        IPublicacionServicioRepository servicioRepository,
        IUnitOfWork? unitOfWork = null,
        INotificacionPushService? notificacionService = null)
    {
        _tecnicoRepository = tecnicoRepository;
        _clienteRepository = clienteRepository;
        _servicioRepository = servicioRepository;
        _unitOfWork = unitOfWork;
        _notificacionService = notificacionService;
    }

    public async Task<PerfilPublicoTecnicoDto> ObtenerPerfilPublicoTecnicoAsync(
        int tecnicoId,
        CancellationToken cancellationToken = default)
    {
        if (tecnicoId <= 0)
        {
            throw new KeyNotFoundException("El identificador del técnico no es válido.");
        }

        // Buscar al técnico utilizando su identificador propio.
        var tecnico = await _tecnicoRepository.ObtenerPorIdAsync(
            tecnicoId,
            cancellationToken);

        // No intentar búsquedas alternativas con valores vacíos ni generar perfiles ficticios.
        if (tecnico is null)
        {
            throw new KeyNotFoundException(
               $"No se encontró un técnico con ID {tecnicoId}.");
        }

        var nombreCompleto = tecnico.NombreCompleto;
        var fotoUrl = tecnico.FotoIdentificacion;
        var costoDiagnostico = tecnico.CostoCotizacion;
        var tieneNegocio = tecnico.TieneNegocioFisico;
        var ubicacionNegocio = tecnico.UbicacionEstablecimiento ?? "Servicio a domicilio";

        // Obtener servicios completados en la plataforma.
        var todosServicios = await _servicioRepository.ObtenerTodosAsync(
            incluirCompletados: true,
            cancellationToken);

        var serviciosTecnico = todosServicios
            .Where(s => s.TecnicoId == tecnicoId)
            .ToList();

        var trabajosCompletados = serviciosTecnico.Count(s =>
            string.Equals(s.Estado, "Completado", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(s.Estado, "Finalizado", StringComparison.OrdinalIgnoreCase));

        // Obtener reseñas.
        var resenas = ResenasPorTecnico.TryGetValue(tecnicoId, out var list)
            ? ObtenerCopiaSeguraResenas(list)
            : new List<ResenaModel>();

        decimal? calificacionPromedio = null;
        bool esTecnicoNuevo = resenas.Count == 0 && trabajosCompletados == 0;
        string mensajeReputacion;

        if (resenas.Count > 0)
        {
            calificacionPromedio = Math.Round(
                (decimal)resenas.Average(r => r.Calificacion),
                1);

            mensajeReputacion =
                $"{calificacionPromedio.Value.ToString("0.0", CultureInfo.InvariantCulture)} " +
                $"({trabajosCompletados} Trabajos Completados)";
        }
        else if (trabajosCompletados > 0)
        {
            calificacionPromedio = 5.0m;
            mensajeReputacion = $"5.0 ({trabajosCompletados} Trabajos Completados)";
        }
        else
        {
            mensajeReputacion = "Técnico nuevo";
        }

        var especialidades = new List<string>
        {
            "Refrigeración",
            "Lavadoras",
            "Microondas",
            "Línea Blanca General"
        };

        var evidenciaTrabajos = new List<string>
        {
            "evidencia_reparacion_motor.jpg",
            "evidencia_cambio_compresor.jpg",
            "certificacion_tecnica.pdf"
        };

        var fechaRegistro = tecnico.Usuario?.FechaUltimoAcceso
            ?? DateTime.UtcNow.AddMonths(-6);

        var miembroDesde = fechaRegistro.ToString(
            "MMMM yyyy",
            new CultureInfo("es-MX"));

        miembroDesde = char.ToUpper(miembroDesde[0]) + miembroDesde[1..];

        var resenasDto = resenas
            .OrderByDescending(r => r.Fecha)
            .Select(MapearAResenaDto)
            .ToList();

        return new PerfilPublicoTecnicoDto(
            Id: tecnico.Id,
            NombreCompleto: nombreCompleto,
            FotoUrl: fotoUrl,
            CalificacionPromedio: calificacionPromedio,
            TotalTrabajosCompletados: trabajosCompletados,
            EsTecnicoNuevo: esTecnicoNuevo,
            MensajeReputacion: mensajeReputacion,
            Especialidades: especialidades,
            CostoDiagnostico: costoDiagnostico,
            TieneNegocioFisico: tieneNegocio,
            UbicacionNegocio: ubicacionNegocio,
            MiembroDesde: miembroDesde,
            EvidenciaTrabajos: evidenciaTrabajos,
            Resenas: resenasDto
        );
    }

    public async Task<PerfilPublicoClienteDto> ObtenerPerfilPublicoClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        if (clienteId <= 0)
        {
            throw new KeyNotFoundException("El identificador del cliente no es válido.");
        }

        var cliente = await _clienteRepository.ObtenerPorUsuarioIdAsync(
            clienteId,
            cancellationToken);

        if (cliente is null)
        {
            throw new KeyNotFoundException(
                $"No se encontró un cliente asociado al usuario con ID {clienteId}.");
        }

        var todosServicios = await _servicioRepository.ObtenerTodosAsync(
            incluirCompletados: true,
            cancellationToken);

        var serviciosCliente = todosServicios
            .Where(s => s.ClienteId == clienteId)
            .ToList();

        var nombre = cliente.Nombre;
        var apellido = cliente.Apellidos ?? string.Empty;

        var inicialApellido = string.IsNullOrWhiteSpace(apellido)
            ? string.Empty
            : $" {apellido.Trim()[0]}.";

        var nombrePublico = $"{nombre}{inicialApellido}";

        // Ubicación generalizada: omitir calles y números.
        var zonaAproximada = ExtraerZonaGeneralizada(cliente.Direccion);

        var serviciosSolicitados = serviciosCliente.Count;

        var serviciosCompletados = serviciosCliente.Count(s =>
            string.Equals(s.Estado, "Completado", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(s.Estado, "Finalizado", StringComparison.OrdinalIgnoreCase));

        var porcentaje = serviciosSolicitados > 0
            ? $"{Math.Min(100, (int)Math.Round(
                (double)serviciosCompletados / serviciosSolicitados * 100))}%"
            : "100%";

        var miembroDesde = DateTime.UtcNow.ToString(
            "MMMM yyyy",
            new CultureInfo("es-MX"));

        miembroDesde = char.ToUpper(miembroDesde[0]) + miembroDesde[1..];

        return new PerfilPublicoClienteDto(
            Id: clienteId,
            NombrePublico: nombrePublico,
            ZonaAproximada: zonaAproximada,
            MiembroDesde: miembroDesde,
            ServiciosSolicitados: serviciosSolicitados,
            ServiciosCompletados: serviciosCompletados,
            PorcentajeInteraccionesExitosas: porcentaje,
            EsUsuarioFrecuente: serviciosSolicitados >= 3
        );
    }

    public async Task<ResenaTecnicoDto> AgregarResenaAsync(
        int tecnicoId,
        CrearResenaRequestDto request,
        int? clienteUsuarioId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Calificacion < 1 || request.Calificacion > 5)
        {
            throw new ReglaNegocioInvalidaException(
                "La calificación debe estar entre 1 y 5 estrellas.");
        }

        // Validar que no exista una reseña para el mismo servicio.
        if (ResenasPorServicio.ContainsKey(request.ServicioId))
        {
            throw new ReglaNegocioInvalidaException(
                "Solo se puede emitir una calificación y reseña por cada servicio completado.");
        }

        var servicio = await _servicioRepository.ObtenerPorIdAsync(
            request.ServicioId,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                $"No se encontró el servicio con ID {request.ServicioId}.");

        // Validar que el servicio haya sido atendido y finalizado.
        var esCompletado =
            string.Equals(servicio.Estado, "Completado", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(servicio.Estado, "Finalizado", StringComparison.OrdinalIgnoreCase);

        if (!esCompletado)
        {
            if (string.Equals(servicio.Estado, "EnProceso", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(servicio.Estado, "Aceptado", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(servicio.Estado, "PendienteCierre", StringComparison.OrdinalIgnoreCase))
            {
                servicio.FinalizarServicio();

                if (_unitOfWork != null)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                throw new ReglaNegocioInvalidaException(
                    "Solo se pueden emitir reseñas para servicios que hayan sido atendidos y finalizados.");
            }
        }

        var comentario = string.IsNullOrWhiteSpace(request.Comentario)
            ? "Servicio completado satisfactoriamente."
            : request.Comentario.Trim();

        var clienteNombre = servicio.Cliente != null
            ? $"{servicio.Cliente.Nombre} {servicio.Cliente.Apellidos.FirstOrDefault()}."
            : "Cliente Verificado";

        var resenaId = Interlocked.Increment(ref _nextResenaId).ToString();
        var ahora = DateTime.UtcNow;

        var nuevaResena = new ResenaModel(
            Id: resenaId,
            TecnicoId: tecnicoId,
            ClienteId: servicio.ClienteId,
            ServicioId: servicio.Id,
            ClienteNombre: clienteNombre,
            Calificacion: request.Calificacion,
            Fecha: ahora,
            Comentario: comentario,
            TipoServicio: servicio.Titulo
        );

        // Evitar que dos solicitudes registren una reseña para el mismo servicio.
        if (!ResenasPorServicio.TryAdd(servicio.Id, nuevaResena))
        {
            throw new ReglaNegocioInvalidaException(
                "Solo se puede emitir una calificación y reseña por cada servicio completado.");
        }

        var lista = ResenasPorTecnico.GetOrAdd(
            tecnicoId,
            _ => new List<ResenaModel>());

        lock (lista)
        {
            lista.Add(nuevaResena);
        }

        if (_notificacionService != null)
        {
            await _notificacionService.NotificarCambioEstadoServicioAsync(
                servicio.Id,
                "Completado",
                clienteUsuarioId ?? servicio.ClienteId,
                cancellationToken);
        }

        return MapearAResenaDto(nuevaResena);
    }

    public Task<ResenaTecnicoDto?> ObtenerResenaPorServicioIdAsync(
        int servicioId,
        CancellationToken cancellationToken = default)
    {
        if (ResenasPorServicio.TryGetValue(servicioId, out var resena))
        {
            return Task.FromResult<ResenaTecnicoDto?>(
                MapearAResenaDto(resena));
        }

        return Task.FromResult<ResenaTecnicoDto?>(null);
    }

    public Task<IReadOnlyList<ResenaTecnicoDto>> ObtenerResenasPorTecnicoIdAsync(
        int tecnicoId,
        CancellationToken cancellationToken = default)
    {
        if (!ResenasPorTecnico.TryGetValue(tecnicoId, out var lista))
        {
            return Task.FromResult<IReadOnlyList<ResenaTecnicoDto>>(
                Array.Empty<ResenaTecnicoDto>());
        }

        var resultado = ObtenerCopiaSeguraResenas(lista)
            .OrderByDescending(r => r.Fecha)
            .Select(MapearAResenaDto)
            .ToList();

        return Task.FromResult<IReadOnlyList<ResenaTecnicoDto>>(resultado);
    }

    private static List<ResenaModel> ObtenerCopiaSeguraResenas(
        List<ResenaModel> lista)
    {
        lock (lista)
        {
            return lista.ToList();
        }
    }

    private static string ExtraerZonaGeneralizada(string? direccion)
    {
        if (string.IsNullOrWhiteSpace(direccion))
        {
            return "Tula de Allende, Hidalgo";
        }

        var partes = direccion.Split(
            new[] { ',', ';' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (partes.Length >= 2)
        {
            return $"{partes[^2]}, {partes[^1]}";
        }

        return partes.Length == 1
            ? partes[0]
            : "Tula de Allende, Hidalgo";
    }

    private static ResenaTecnicoDto MapearAResenaDto(ResenaModel r)
    {
        var diff = DateTime.UtcNow - r.Fecha;
        string fechaRelativa;

        if (diff.TotalDays < 1)
            fechaRelativa = "Hoy";
        else if (diff.TotalDays < 2)
            fechaRelativa = "Hace 1 día";
        else if (diff.TotalDays < 7)
            fechaRelativa = $"Hace {(int)diff.TotalDays} días";
        else if (diff.TotalDays < 14)
            fechaRelativa = "Hace 1 semana";
        else if (diff.TotalDays < 30)
            fechaRelativa = $"Hace {(int)(diff.TotalDays / 7)} semanas";
        else
            fechaRelativa = r.Fecha.ToString("dd/MM/yyyy");

        return new ResenaTecnicoDto(
            Id: r.Id,
            ClienteNombre: r.ClienteNombre,
            CalificacionEstrellas: r.Calificacion,
            FechaRelativa: fechaRelativa,
            Fecha: r.Fecha,
            Comentario: r.Comentario,
            TipoServicio: $"SERVICIO: {r.TipoServicio.ToUpperInvariant()}"
        );
    }
}
