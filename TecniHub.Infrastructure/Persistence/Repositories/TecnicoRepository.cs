using Microsoft.EntityFrameworkCore;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Domain.Entities;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Infrastructure.Persistence.Repositories;

public class TecnicoRepository : ITecnicoRepository
{
    private readonly TecniHubDbContext _context;

    public TecnicoRepository(TecniHubDbContext context)
    {
        _context = context;
    }

    public async Task<Tecnico?> ObtenerPorUsuarioIdAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tecnicos
            .FirstOrDefaultAsync(
                t => t.UsuarioId == usuarioId,
                cancellationToken);
    }

    public async Task<Tecnico?> ObtenerPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var correoVo = new CorreoElectronico(email);

        return await _context.Tecnicos
            .FirstOrDefaultAsync(
                t => t.Correo == correoVo,
                cancellationToken);
    }

    public async Task<bool> ExisteEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var correoVo = new CorreoElectronico(email);

        return await _context.Tecnicos
            .AnyAsync(
                t => t.Correo == correoVo,
                cancellationToken);
    }

    public async Task AgregarTecnicoAsync(
        Tecnico tecnico,
        CancellationToken cancellationToken = default)
    {
        await _context.Tecnicos.AddAsync(
            tecnico,
            cancellationToken);
    }
}
