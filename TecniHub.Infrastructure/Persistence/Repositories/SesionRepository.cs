using TecniHub.Application.Common.Interfaces;
using TecniHub.Domain.Entities;

namespace TecniHub.Infrastructure.Persistence.Repositories;

public class SesionRepository : ISesionRepository
{
    private readonly TecniHubDbContext _context;

    public SesionRepository(TecniHubDbContext context)
    {
        _context = context;
    }

    public async Task CrearSesionAsync(Sesion sesion, CancellationToken cancellationToken = default)
    {
        await _context.Sesiones.AddAsync(sesion, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}