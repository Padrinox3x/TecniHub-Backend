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

    async Task<(int TotalSolicitados, int TotalCompletados)> ObtenerEstadisticasClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        var servicios = await ObtenerPorClienteIdAsync(clienteId, cancellationToken);
        var completados = servicios.Count(s =>
            string.Equals(s.Estado, "Completado", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(s.Estado, "Finalizado", StringComparison.OrdinalIgnoreCase));

        return (servicios.Count, completados);
    }
}
