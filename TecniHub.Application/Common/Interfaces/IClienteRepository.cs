using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TecniHub.Application.DTOs.Perfil;
using TecniHub.Domain.Entities;

namespace TecniHub.Application.Common.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult<Cliente?>(null);

    Task<Cliente?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AgregarClienteAsync(Cliente cliente, CancellationToken cancellationToken = default);

    async Task<ClientePerfilPublicoData?> ObtenerDatosPerfilPublicoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var cliente = await ObtenerPorIdAsync(id, cancellationToken)
                      ?? await ObtenerPorUsuarioIdAsync(id, cancellationToken);

        return cliente is null
            ? null
            : new ClientePerfilPublicoData(
                cliente.Id,
                cliente.UsuarioId,
                cliente.Nombre,
                cliente.Apellidos,
                cliente.Direccion,
                cliente.Usuario?.FechaUltimoAcceso);
    }
}
