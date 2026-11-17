using Microsoft.EntityFrameworkCore;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Domain.Entities;

namespace TecniHub.Infrastructure.Persistence.Repositories;

public class PublicacionPiezaRepository : IPublicacionPiezaRepository
{
    private readonly TecniHubDbContext _context;

    public PublicacionPiezaRepository(TecniHubDbContext context)
    {
        _context = context;
    }

    public async Task<PublicacionPieza?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.PublicacionesPieza
            .Include(p => p.Tecnico)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<PublicacionPieza>> ObtenerTodosAsync(
        bool incluirCompletados = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PublicacionesPieza
            .Include(p => p.Tecnico)
            .AsQueryable();

        if (!incluirCompletados)
        {
            query = query.Where(p => p.EstadoDisponibilidad != "Completado" && p.EstadoDisponibilidad != "Finalizado" && p.EstadoDisponibilidad != "Vendido");
        }

        return await query
            .OrderByDescending(p => p.FechaPublicacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PublicacionPieza>> ObtenerPorTecnicoIdAsync(
        int tecnicoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PublicacionesPieza
            .Include(p => p.Tecnico)
            .Where(p => p.TecnicoId == tecnicoId)
            .OrderByDescending(p => p.FechaPublicacion)
            .ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(
        PublicacionPieza publicacion,
        CancellationToken cancellationToken = default)
    {
        await _context.PublicacionesPieza.AddAsync(publicacion, cancellationToken);
    }
}
