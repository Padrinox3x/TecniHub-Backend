using Microsoft.EntityFrameworkCore;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Domain.Entities;

namespace TecniHub.Infrastructure.Persistence.Repositories;

public class PublicacionServicioRepository : IPublicacionServicioRepository
{
    private readonly TecniHubDbContext _context;

    public PublicacionServicioRepository(TecniHubDbContext context)
    {
        _context = context;
    }

    public async Task<PublicacionServicio?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.PublicacionesServicio
            .Include(s => s.Cliente)
            .Include(s => s.Tecnico)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<PublicacionServicio>> ObtenerTodosAsync(
        bool incluirCompletados = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PublicacionesServicio
            .Include(s => s.Cliente)
            .Include(s => s.Tecnico)
            .AsQueryable();

        if (!incluirCompletados)
        {
            query = query.Where(s => s.Estado != "Completado" && s.Estado != "Finalizado");
        }

        return await query
            .OrderByDescending(s => s.FechaPublicacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PublicacionServicio>> ObtenerPorClienteIdAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PublicacionesServicio
            .Include(s => s.Cliente)
            .Include(s => s.Tecnico)
            .Where(s => s.ClienteId == clienteId)
            .OrderByDescending(s => s.FechaPublicacion)
            .ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(
        PublicacionServicio publicacion,
        CancellationToken cancellationToken = default)
    {
        await _context.PublicacionesServicio.AddAsync(publicacion, cancellationToken);
    }

    public async Task<(int TotalSolicitados, int TotalCompletados)> ObtenerEstadisticasClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        var estados = await _context.PublicacionesServicio
            .AsNoTracking()
            .Where(s => s.ClienteId == clienteId)
            .Select(s => (string?)s.Estado ?? string.Empty)
            .ToListAsync(cancellationToken);

        var completados = estados.Count(e =>
            string.Equals(e, "Completado", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e, "Finalizado", StringComparison.OrdinalIgnoreCase));

        return (estados.Count, completados);
    }
}
