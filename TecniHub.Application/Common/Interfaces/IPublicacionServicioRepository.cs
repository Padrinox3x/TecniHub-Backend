using TecniHub.Domain.Entities;

namespace TecniHub.Application.Common.Interfaces;

public interface IPublicacionServicioRepository
{
    Task<PublicacionServicio?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<List<PublicacionServicio>> ObtenerTodosAsync(
        bool incluirCompletados = false,
        CancellationToken cancellationToken = default);

    Task<List<PublicacionServicio>> ObtenerPorClienteIdAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        PublicacionServicio publicacion,
        CancellationToken cancellationToken = default);
}
