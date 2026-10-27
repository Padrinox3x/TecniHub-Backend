using TecniHub.Domain.Entities;

namespace TecniHub.Application.Common.Interfaces;

public interface IIntentoLoginRepository
{
    Task RegistrarIntentoAsync(IntentoLogin intento, CancellationToken cancellationToken = default);
}