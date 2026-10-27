using TecniHub.Application.Common.Interfaces;
using TecniHub.Domain.Entities;

namespace TecniHub.Infrastructure.Persistence.Repositories;

public class IntentoLoginRepository : IIntentoLoginRepository
{
    private readonly TecniHubDbContext _context;

    public IntentoLoginRepository(TecniHubDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarIntentoAsync(IntentoLogin intento, CancellationToken cancellationToken = default)
    {
        await _context.IntentosLogin.AddAsync(intento, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}