using TecniHub.Application.DTOs.Tablon;

namespace TecniHub.Application.Common.Interfaces;

public interface ITablonService
{
    Task<List<TablonItemDto>> ObtenerTablonAsync(
        TablonFiltroDto? filtro = null,
        CancellationToken cancellationToken = default);

    Task<TablonItemDto> ObtenerPublicacionServicioPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<TablonItemDto> ObtenerPublicacionPiezaPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<TablonItemDto> CrearPublicacionServicioAsync(
        int usuarioId,
        CrearPublicacionServicioRequestDto request,
        CancellationToken cancellationToken = default);

    Task<TablonItemDto> CrearPublicacionPiezaAsync(
        int usuarioId,
        CrearPublicacionPiezaRequestDto request,
        CancellationToken cancellationToken = default);

    Task<TablonItemDto> AceptarServicioAsync(
        int usuarioId,
        int publicacionId,
        CancellationToken cancellationToken = default);

    Task<TablonItemDto> FinalizarServicioAsync(
        int usuarioId,
        int publicacionId,
        CancellationToken cancellationToken = default);

    Task<TablonItemDto> FinalizarPiezaAsync(
        int usuarioId,
        int publicacionId,
        CancellationToken cancellationToken = default);
}
