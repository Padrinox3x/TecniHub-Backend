using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TecniHub.Domain.Entities;

namespace TecniHub.Application.Common.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AgregarClienteAsync(Cliente cliente, CancellationToken cancellationToken = default);
}
