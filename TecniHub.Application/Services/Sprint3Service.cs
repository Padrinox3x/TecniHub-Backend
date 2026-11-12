using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Sprint3;

namespace TecniHub.Application.Services;

public sealed class Sprint3Service(ISprint3Repository repository) : ISprint3Service
{
    public Task<ServicioCreadoDto> CrearServicioAsync(CrearServicioRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ClienteId == Guid.Empty) throw new ArgumentException("ClienteId es obligatorio.");
        if (request.CategoriaId <= 0) throw new ArgumentException("CategoriaId debe ser válido.");
        return repository.CrearServicioAsync(request, cancellationToken);
    }

    public Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken = default) =>
        repository.ObtenerCategoriasAsync(cancellationToken);

    public Task<PiezaPublicadaDto> PublicarPiezaAsync(PublicarPiezaRequestDto request, string? ipOrigen, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.TecnicoId == Guid.Empty) throw new ArgumentException("TecnicoId es obligatorio.");
        if (request.CategoriaId <= 0) throw new ArgumentException("CategoriaId debe ser válido.");
        if (request.Modalidad is not ("VENTA" or "INTERCAMBIO" or "VENTA_E_INTERCAMBIO"))
            throw new ArgumentException("Modalidad no reconocida.");
        if (request.Condicion is not ("NUEVO" or "USADO_EXCELENTE" or "USADO_BUENO" or "PARA_REPARAR"))
            throw new ArgumentException("Condición no reconocida.");
        if (request.Modalidad is "VENTA" or "VENTA_E_INTERCAMBIO" && request.Precio is null or <= 0)
            throw new ArgumentException("La modalidad de venta requiere precio mayor que cero.");
        if (request.Modalidad is "INTERCAMBIO" or "VENTA_E_INTERCAMBIO" && string.IsNullOrWhiteSpace(request.PiezaIntercambioDeseada))
            throw new ArgumentException("La modalidad de intercambio requiere indicar la pieza deseada.");
        return repository.PublicarPiezaAsync(request, ipOrigen, cancellationToken);
    }

    public Task<IReadOnlyList<PiezaDto>> BuscarPiezasAsync(BuscarPiezasRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return repository.BuscarPiezasAsync(request, cancellationToken);
    }

    public async Task<ItinerarioProgramadoDto> ProgramarItinerarioAsync(ProgramarItinerarioRequestDto request, string? ipOrigen, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ServicioId == Guid.Empty || request.TecnicoId == Guid.Empty || request.ActorId == Guid.Empty)
            throw new ArgumentException("ServicioId, TecnicoId y ActorId son obligatorios.");
        if (request.Fecha is null || request.HoraInicio is null || request.HoraFinEstimada is null)
            throw new ArgumentException("Fecha, hora de inicio y hora estimada de fin son obligatorias.");
        if (request.HoraFinEstimada <= request.HoraInicio)
            throw new ArgumentException("La hora estimada de fin debe ser posterior a la hora de inicio.");
        await repository.ProgramarItinerarioAsync(request, ipOrigen, cancellationToken);
        return new ItinerarioProgramadoDto(request.ServicioId, request.Fecha.Value, request.HoraInicio.Value,
            request.HoraFinEstimada.Value, "PROGRAMADO");
    }
}
