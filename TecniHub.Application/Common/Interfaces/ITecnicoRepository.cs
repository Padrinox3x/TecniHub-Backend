using TecniHub.Domain.Entities;

namespace TecniHub.Application.Common.Interfaces;

public interface ITecnicoRepository
{
    Task<Tecnico?> ObtenerPorUsuarioIdAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<Tecnico?> ObtenerPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task AgregarTecnicoAsync(
        Tecnico tecnico,
        CancellationToken cancellationToken = default);
}
