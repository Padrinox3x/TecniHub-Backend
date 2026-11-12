using TecniHub.Application.DTOs.Sprint3;

namespace TecniHub.Application.Common.Interfaces;

public interface ISprint3Repository
{
    Task<ServicioCreadoDto> CrearServicioAsync(CrearServicioRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken);
    Task<PiezaPublicadaDto> PublicarPiezaAsync(PublicarPiezaRequestDto request, string? ipOrigen, CancellationToken cancellationToken);
    Task<IReadOnlyList<PiezaDto>> BuscarPiezasAsync(BuscarPiezasRequestDto request, CancellationToken cancellationToken);
    Task ProgramarItinerarioAsync(ProgramarItinerarioRequestDto request, string? ipOrigen, CancellationToken cancellationToken);
}
