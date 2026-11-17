using System.Collections.Concurrent;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Itinerarios;
using TecniHub.Domain.Exceptions;

namespace TecniHub.Application.Services;

public sealed class ItinerarioService : IItinerarioService
{
    private readonly IPublicacionServicioRepository _servicioRepository;
    private readonly ITecnicoRepository _tecnicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    // Almacén en memoria thread-safe para citas de itinerario técnico
    private static readonly ConcurrentDictionary<string, CitaItinerarioRegistro> ItinerariosStore = new();
    private static int _nextId = 1;

    private sealed record CitaItinerarioRegistro(
        int Id,
        int ServicioId,
        int TecnicoId,
        DateOnly Fecha,
        TimeOnly HoraInicio,
        TimeOnly HoraFinEstimada,
        string? Notas,
        string Estado,
        DateTime CreatedAt
    );

    public ItinerarioService(
        IPublicacionServicioRepository servicioRepository,
        ITecnicoRepository tecnicoRepository,
        IUnitOfWork unitOfWork)
    {
        _servicioRepository = servicioRepository;
        _tecnicoRepository = tecnicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ItinerarioProgramadoDto> ProgramarItinerarioAsync(
        ProgramarItinerarioRequestDto request,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Fecha))
            throw new ReglaNegocioInvalidaException("La fecha de la cita es obligatoria.");

        if (string.IsNullOrWhiteSpace(request.HoraInicio) || string.IsNullOrWhiteSpace(request.HoraFinEstimada))
            throw new ReglaNegocioInvalidaException("Las horas de inicio y fin estimada son obligatorias.");

        if (!DateOnly.TryParse(request.Fecha, out var fecha))
            throw new ReglaNegocioInvalidaException($"Formato de fecha inválido: {request.Fecha}");

        if (!TimeOnly.TryParse(request.HoraInicio, out var horaInicio))
            throw new ReglaNegocioInvalidaException($"Formato de hora de inicio inválido: {request.HoraInicio}");

        if (!TimeOnly.TryParse(request.HoraFinEstimada, out var horaFin))
            throw new ReglaNegocioInvalidaException($"Formato de hora de fin inválido: {request.HoraFinEstimada}");

        if (horaFin <= horaInicio)
            throw new ReglaNegocioInvalidaException("La hora estimada de fin debe ser posterior a la hora de inicio.");

        var servicioId = ResolverServicioId(request.ServicioId);
        var tecnicoId = await ResolverTecnicoIdAsync(request.TecnicoId, usuarioIdAutenticado, cancellationToken);

        var servicio = await _servicioRepository.ObtenerPorIdAsync(servicioId, cancellationToken);
        if (servicio is null)
        {
            // Si no se encuentra con el ID exacto, permitimos buscar el primer servicio aceptado o validar
            servicio = (await _servicioRepository.ObtenerTodosAsync(incluirCompletados: false, cancellationToken))
                .FirstOrDefault(s => s.TecnicoId == tecnicoId || s.Id == servicioId);
        }

        if (servicio != null)
        {
            // Regla de Negocio: Solo servicios en estado Aceptado/EnProceso y asignados a este técnico
            var estado = servicio.Estado?.Trim() ?? string.Empty;
            var esAceptado = string.Equals(estado, "EnProceso", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(estado, "Aceptado", StringComparison.OrdinalIgnoreCase);

            if (!esAceptado)
            {
                throw new InvalidOperationException("El servicio debe encontrarse en estado 'Aceptado' (verde) para poder agregarlo al itinerario.");
            }

            if (servicio.TecnicoId.HasValue && servicio.TecnicoId.Value != tecnicoId)
            {
                throw new InvalidOperationException("Este servicio no está asignado a tu cuenta de técnico.");
            }
        }

        // Criterio 2: Validar si ya tiene un servicio programado cerca de esta hora en la misma fecha
        string? advertencia = null;
        var citasMismoDia = ItinerariosStore.Values
            .Where(c => c.TecnicoId == tecnicoId && c.Fecha == fecha && c.Estado != "Completado" && c.ServicioId != servicioId)
            .ToList();

        var hayTraslape = citasMismoDia.Any(c => horaInicio < c.HoraFinEstimada && horaFin > c.HoraInicio);
        if (hayTraslape)
        {
            advertencia = "Ya tienes un servicio programado cerca de esta hora.";
        }

        var key = $"{tecnicoId}_{servicioId}";
        var nuevoId = Interlocked.Increment(ref _nextId);
        var registro = new CitaItinerarioRegistro(
            Id: nuevoId,
            ServicioId: servicioId,
            TecnicoId: tecnicoId,
            Fecha: fecha,
            HoraInicio: horaInicio,
            HoraFinEstimada: horaFin,
            Notas: request.Notas?.Trim(),
            Estado: "Programado",
            CreatedAt: DateTime.UtcNow
        );

        ItinerariosStore[key] = registro;

        return new ItinerarioProgramadoDto(
            ServicioId: request.ServicioId,
            Fecha: fecha.ToString("yyyy-MM-dd"),
            HoraInicio: horaInicio.ToString("HH:mm:ss"),
            HoraFinEstimada: horaFin.ToString("HH:mm:ss"),
            Estado: "Programado",
            Advertencia: advertencia
        );
    }

    public async Task<IReadOnlyList<ItinerarioItemDto>> ObtenerItinerarioAsync(
        int? tecnicoId = null,
        int? usuarioIdAutenticado = null,
        bool incluirCompletados = false,
        CancellationToken cancellationToken = default)
    {
        var idTecnico = tecnicoId;
        if (!idTecnico.HasValue && usuarioIdAutenticado.HasValue)
        {
            var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioIdAutenticado.Value, cancellationToken);
            idTecnico = tecnico?.Id;
        }

        var query = ItinerariosStore.Values.AsEnumerable();

        if (idTecnico.HasValue)
        {
            query = query.Where(c => c.TecnicoId == idTecnico.Value);
        }

        if (!incluirCompletados)
        {
            query = query.Where(c => c.Estado != "Completado" && c.Estado != "Finalizado");
        }

        // Ordenamiento: Sort de las tarjetas desde la fecha y hora más próxima hasta la más lejana
        var citasOrdenadas = query
            .OrderBy(c => c.Fecha)
            .ThenBy(c => c.HoraInicio)
            .ToList();

        var resultado = new List<ItinerarioItemDto>();
        foreach (var cita in citasOrdenadas)
        {
            var servicio = await _servicioRepository.ObtenerPorIdAsync(cita.ServicioId, cancellationToken);
            var tecnico = await _tecnicoRepository.ObtenerPorEmailAsync(string.Empty, cancellationToken); // o por ID

            resultado.Add(new ItinerarioItemDto(
                Id: cita.Id,
                ServicioId: cita.ServicioId,
                CodigoFolio: $"SRV-{cita.ServicioId:D5}",
                TecnicoId: cita.TecnicoId,
                TecnicoNombre: tecnico?.NombreCompleto,
                TituloServicio: servicio?.Titulo ?? $"Servicio #{cita.ServicioId}",
                TipoElectrodomestico: servicio?.TipoElectrodomestico ?? "Electrodoméstico",
                Ubicacion: servicio?.UbicacionAproximada ?? "No especificada",
                FechaProgramada: cita.Fecha.ToString("yyyy-MM-dd"),
                HoraInicio: cita.HoraInicio.ToString("HH:mm"),
                HoraFinEstimada: cita.HoraFinEstimada.ToString("HH:mm"),
                Notas: cita.Notas,
                Estado: cita.Estado,
                EsCompletado: string.Equals(cita.Estado, "Completado", StringComparison.OrdinalIgnoreCase)
            ));
        }

        return resultado;
    }

    public async Task FinalizarServicioAgendadoAsync(
        int servicioId,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        // Buscar en el almacén de itinerarios
        var match = ItinerariosStore.FirstOrDefault(kv => kv.Value.ServicioId == servicioId);
        if (!match.Equals(default(KeyValuePair<string, CitaItinerarioRegistro>)))
        {
            var r = match.Value;
            ItinerariosStore[match.Key] = r with { Estado = "Completado" };
        }

        // Finalizar el servicio en la base de datos
        var servicio = await _servicioRepository.ObtenerPorIdAsync(servicioId, cancellationToken);
        if (servicio != null)
        {
            servicio.FinalizarServicio();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<int> ResolverTecnicoIdAsync(string tecnicoIdStr, int? usuarioId, CancellationToken cancellationToken)
    {
        if (usuarioId.HasValue)
        {
            var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioId.Value, cancellationToken);
            if (tecnico != null) return tecnico.Id;
        }

        if (int.TryParse(tecnicoIdStr, out var idInt) && idInt > 0)
        {
            return idInt;
        }

        return 1;
    }

    private static int ResolverServicioId(string servicioIdStr)
    {
        if (int.TryParse(servicioIdStr, out var idInt) && idInt > 0)
        {
            return idInt;
        }

        // Si viene un GUID, generar un hash consistente a int positivo
        return Math.Abs(servicioIdStr.GetHashCode() % 10000) + 1;
    }
}
