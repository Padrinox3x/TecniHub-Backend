
using TecniHub.Application.DTOs.Auth;

namespace TecniHub.Application.Common.Interfaces;

public interface ILoginService
{
    Task<AuthResponseDto> IniciarSesionAsync(
        LoginRequestDto request,
        string? ipOrigen,
        string? userAgent,
        CancellationToken cancellationToken = default);
}
