
using Microsoft.EntityFrameworkCore;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Domain.Entities;

namespace TecniHub.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly TecniHubDbContext _context;

    public UsuarioRepository(TecniHubDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(
                u => u.Correo == email,
                cancellationToken);
    }

    public async Task<bool> ExisteEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AnyAsync(
                u => u.Correo == email,
                cancellationToken);
    }

    public async Task AgregarUsuarioAsync(
        Usuario usuario,
        CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(
            usuario,
            cancellationToken);
    }
}
