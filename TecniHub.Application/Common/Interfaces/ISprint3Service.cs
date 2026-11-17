using TecniHub.Application.DTOs.Sprint3;

namespace TecniHub.Application.Common.Interfaces;

public interface ISprint3Service
{
    Task<ServicioCreadoDto> CrearServicioAsync(CrearServicioRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken = default);
    Task<PiezaPublicadaDto> PublicarPiezaAsync(PublicarPiezaRequestDto request, string? ipOrigen, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PiezaDto>> BuscarPiezasAsync(BuscarPiezasRequestDto request, CancellationToken cancellationToken = default);
    Task<ItinerarioProgramadoDto> ProgramarItinerarioAsync(ProgramarItinerarioRequestDto request, string? ipOrigen, CancellationToken cancellationToken = default);
}
