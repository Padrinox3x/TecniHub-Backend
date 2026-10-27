using TecniHub.Domain.Entities;

namespace TecniHub.Application.Common.Interfaces;

public interface ISesionRepository
{
    Task CrearSesionAsync(Sesion sesion, CancellationToken cancellationToken = default);
}