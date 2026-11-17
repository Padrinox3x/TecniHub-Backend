using TecniHub.Application.DTOs.Perfil;

namespace TecniHub.Application.Common.Interfaces;

public interface IPerfilPublicoService
{
    Task<PerfilPublicoTecnicoDto> ObtenerPerfilPublicoTecnicoAsync(
        int tecnicoId,
        CancellationToken cancellationToken = default);

    Task<PerfilPublicoClienteDto> ObtenerPerfilPublicoClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<ResenaTecnicoDto> AgregarResenaAsync(
        int tecnicoId,
        CrearResenaRequestDto request,
        int? clienteUsuarioId = null,
        CancellationToken cancellationToken = default);

    Task<ResenaTecnicoDto?> ObtenerResenaPorServicioIdAsync(
        int servicioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResenaTecnicoDto>> ObtenerResenasPorTecnicoIdAsync(
        int tecnicoId,
        CancellationToken cancellationToken = default);
}
