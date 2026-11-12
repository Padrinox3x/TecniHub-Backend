using TecniHub.Domain.Entities;

namespace TecniHub.Application.Common.Interfaces;

public interface IPublicacionPiezaRepository
{
    Task<PublicacionPieza?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<List<PublicacionPieza>> ObtenerTodosAsync(
        bool incluirCompletados = false,
        CancellationToken cancellationToken = default);

    Task<List<PublicacionPieza>> ObtenerPorTecnicoIdAsync(
        int tecnicoId,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        PublicacionPieza publicacion,
        CancellationToken cancellationToken = default);
}
