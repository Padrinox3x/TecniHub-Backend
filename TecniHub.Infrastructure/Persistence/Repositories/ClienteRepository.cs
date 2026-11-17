using Microsoft.EntityFrameworkCore;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Domain.Entities;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Infrastructure.Persistence.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly TecniHubDbContext _context;

    public ClienteRepository(TecniHubDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var correoVo = new CorreoElectronico(email);

        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Correo == correoVo, cancellationToken);
    }

    public async Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var correoVo = new CorreoElectronico(email);

        return await _context.Clientes
            .AnyAsync(c => c.Correo == correoVo, cancellationToken);
    }

    public async Task AgregarClienteAsync(
        Cliente cliente,
        CancellationToken cancellationToken = default)
    {
        await _context.Clientes.AddAsync(cliente, cancellationToken);
    }
}