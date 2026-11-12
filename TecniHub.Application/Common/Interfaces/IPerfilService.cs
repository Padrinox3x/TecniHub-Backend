using TecniHub.Application.DTOs.Perfil;

namespace TecniHub.Application.Common.Interfaces;

public interface IPerfilService
{
    Task<PerfilResponseDto> ObtenerPerfilAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<PerfilResponseDto> EditarPerfilClienteAsync(
        int usuarioId,
        EditarPerfilClienteRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PerfilResponseDto> EditarPerfilTecnicoAsync(
        int usuarioId,
        EditarPerfilTecnicoRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PerfilResponseDto> EditarPerfilAsync(
        int usuarioId,
        EditarPerfilRequestDto request,
        CancellationToken cancellationToken = default);
}
