using Microsoft.EntityFrameworkCore;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Perfil;
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

    public async Task<Cliente?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cliente?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task<ClientePerfilPublicoData?> ObtenerDatosPerfilPublicoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        // Proyección directa sin materializar Value Objects (CorreoElectronico/NumeroTelefono)
        // ni requerir .Include explícito, priorizando coincidencia por Cliente.Id y luego UsuarioId.
        return await _context.Clientes
            .AsNoTracking()
            .Where(c => c.Id == id || c.UsuarioId == id)
            .OrderBy(c => c.Id == id ? 0 : 1)
            .Select(c => new ClientePerfilPublicoData(
                c.Id,
                c.UsuarioId,
                (string?)c.Nombre ?? "Cliente",
                (string?)c.Apellidos ?? string.Empty,
                c.Direccion,
                c.Usuario != null ? c.Usuario.FechaUltimoAcceso : null))
            .FirstOrDefaultAsync(cancellationToken);
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