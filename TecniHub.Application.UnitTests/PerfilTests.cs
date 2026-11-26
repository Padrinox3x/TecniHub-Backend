using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Perfil;
using TecniHub.Application.Services;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Application.UnitTests;

public class PerfilTests
{
    private class FakeUsuarioRepository : IUsuarioRepository
    {
        private readonly List<Usuario> _usuarios = new();

        public void Agregar(Usuario usuario) => _usuarios.Add(usuario);

        public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_usuarios.FirstOrDefault(u => u.Id == id));
        }

        public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_usuarios.FirstOrDefault(u => u.Correo == email.Trim().ToLowerInvariant()));
        }

        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_usuarios.Any(u => u.Correo == email.Trim().ToLowerInvariant()));
        }

        public Task AgregarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            _usuarios.Add(usuario);
            return Task.CompletedTask;
        }
    }

    private class FakeClienteRepository : IClienteRepository
    {
        private readonly List<Cliente> _clientes = new();

        public void Agregar(Cliente cliente) => _clientes.Add(cliente);

        public Task<Cliente?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_clientes.FirstOrDefault(c => c.UsuarioId == usuarioId));
        }

        public Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_clientes.FirstOrDefault(c => c.Correo.Valor == email.Trim().ToLowerInvariant()));
        }

        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_clientes.Any(c => c.Correo.Valor == email.Trim().ToLowerInvariant()));
        }

        public Task AgregarClienteAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            _clientes.Add(cliente);
            return Task.CompletedTask;
        }
    }

    private class FakeTecnicoRepository : ITecnicoRepository
    {
        private readonly List<Tecnico> _tecnicos = new();

        public void Agregar(Tecnico tecnico) => _tecnicos.Add(tecnico);

        public Task<Tecnico?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_tecnicos.FirstOrDefault(t => t.UsuarioId == usuarioId));
        }

        public Task<Tecnico?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_tecnicos.FirstOrDefault(t => t.Correo.Valor == email.Trim().ToLowerInvariant()));
        }

        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_tecnicos.Any(t => t.Correo.Valor == email.Trim().ToLowerInvariant()));
        }

        public Task AgregarTecnicoAsync(Tecnico tecnico, CancellationToken cancellationToken = default)
        {
            _tecnicos.Add(tecnico);
            return Task.CompletedTask;
        }
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    [Fact]
    public async Task ObtenerPerfilAsync_ClienteExistente_RetornaPerfilConDatosCliente()
    {
        // Arrange
        var usuarioRepo = new FakeUsuarioRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var unitOfWork = new FakeUnitOfWork();

        var usuario = new Usuario("cliente@example.com", "hash", "CLIENTE");
        usuarioRepo.Agregar(usuario);

        var cliente = new Cliente(
            usuario,
            "Laura",
            "Márquez",
            new CorreoElectronico("cliente@example.com"),
            "hash",
            new NumeroTelefono("5512345678"),
            "Calle Roble 123");
        clienteRepo.Agregar(cliente);

        var service = new PerfilService(usuarioRepo, clienteRepo, tecnicoRepo, unitOfWork);

        // Act
        var result = await service.ObtenerPerfilAsync(usuario.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cliente@example.com", result.Email);
        Assert.Equal("CLIENTE", result.Rol);
        Assert.NotNull(result.Cliente);
        Assert.Equal("Laura", result.Cliente.Nombre);
        Assert.Equal("Márquez", result.Cliente.Apellidos);
        Assert.Equal("5512345678", result.Cliente.Telefono);
        Assert.Null(result.Tecnico);
    }

    [Fact]
    public async Task EditarPerfilClienteAsync_DatosValidos_ActualizaPerfilCorrectamente()
    {
        // Arrange
        var usuarioRepo = new FakeUsuarioRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var unitOfWork = new FakeUnitOfWork();

        var usuario = new Usuario("cliente@example.com", "hash", "CLIENTE");
        usuarioRepo.Agregar(usuario);

        var cliente = new Cliente(
            usuario,
            "Laura",
            "Márquez",
            new CorreoElectronico("cliente@example.com"),
            "hash",
            new NumeroTelefono("5512345678"),
            "Calle Roble 123");
        clienteRepo.Agregar(cliente);

        var service = new PerfilService(usuarioRepo, clienteRepo, tecnicoRepo, unitOfWork);

        var request = new EditarPerfilClienteRequestDto
        {
            Nombre = "Laura Elena",
            Apellidos = "Márquez Silva",
            Telefono = "5599887766",
            Direccion = "Nueva Dirección 456"
        };

        // Act
        var result = await service.EditarPerfilClienteAsync(usuario.Id, request);

        // Assert
        Assert.NotNull(result.Cliente);
        Assert.Equal("Laura Elena", result.Cliente.Nombre);
        Assert.Equal("Márquez Silva", result.Cliente.Apellidos);
        Assert.Equal("5599887766", result.Cliente.Telefono);
        Assert.Equal("Nueva Dirección 456", result.Cliente.Direccion);
    }

    [Fact]
    public async Task EditarPerfilTecnicoAsync_NegocioFisicoSinUbicacion_LanzaReglaNegocioInvalidaException()
    {
        // Arrange
        var usuarioRepo = new FakeUsuarioRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var unitOfWork = new FakeUnitOfWork();

        var usuario = new Usuario("tecnico@example.com", "hash", "TECNICO");
        usuarioRepo.Agregar(usuario);

        var tecnico = new Tecnico(
            usuario,
            "Mario Tecnico",
            new CorreoElectronico("tecnico@example.com"),
            "hash",
            new NumeroTelefono("5512345678"));
        tecnicoRepo.Agregar(tecnico);

        var service = new PerfilService(usuarioRepo, clienteRepo, tecnicoRepo, unitOfWork);

        var request = new EditarPerfilTecnicoRequestDto
        {
            NombreCompleto = "Mario Tecnico Editado",
            Telefono = "5512345678",
            TieneNegocioFisico = true,
            UbicacionEstablecimiento = null // Invalido
        };

        // Act & Assert
        await Assert.ThrowsAsync<ReglaNegocioInvalidaException>(
            () => service.EditarPerfilTecnicoAsync(usuario.Id, request));
    }
}
