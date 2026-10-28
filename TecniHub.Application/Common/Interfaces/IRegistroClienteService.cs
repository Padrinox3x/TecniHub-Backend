
using TecniHub.Application.DTOs.Auth;

namespace TecniHub.Application.Common.Interfaces;

public interface IRegistroClienteService
{
    Task<AuthResponseDto> RegistrarAsync(
        RegistroClienteRequestDto request,
        CancellationToken cancellationToken = default);
}
