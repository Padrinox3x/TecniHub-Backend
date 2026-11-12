using TecniHub.Application.DTOs.Auth;

namespace TecniHub.Application.Common.Interfaces;

public interface IRegistroTecnicoService
{
    Task<AuthResponseDto> RegistrarAsync(
        RegistroTecnicoRequestDto request,
        CancellationToken cancellationToken = default);
}
