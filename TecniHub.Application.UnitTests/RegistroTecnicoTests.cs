using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Auth;
using TecniHub.Application.Services;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Application.UnitTests;

public class RegistroTecnicoTests
{
    private class FakeUsuarioRepository : IUsuarioRepository
    {
        private readonly List<Usuario> _usuarios = new();

        public Task AgregarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            _usuarios.Add(usuario);
            return Task.CompletedTask;
        }

        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_usuarios.Any(u => u.Correo == email.Trim().ToLowerInvariant()));
        }

        public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_usuarios.FirstOrDefault(u => u.Correo == email.Trim().ToLowerInvariant()));
        }
    }

    private class FakeTecnicoRepository : ITecnicoRepository
    {
        private readonly List<Tecnico> _tecnicos = new();

        public Task AgregarTecnicoAsync(Tecnico tecnico, CancellationToken cancellationToken = default)
        {
            _tecnicos.Add(tecnico);
            return Task.CompletedTask;
        }

        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_tecnicos.Any(t => t.Correo.Valor == email.Trim().ToLowerInvariant()));
        }

        public Task<Tecnico?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_tecnicos.FirstOrDefault(t => t.Correo.Valor == email.Trim().ToLowerInvariant()));
        }
    }

    private class FakeClienteRepository : IClienteRepository
    {
        public Task AgregarClienteAsync(Cliente cliente, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult<Cliente?>(null);
    }

    private class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed_{password}";
        public bool Verify(string password, string passwordHash) => passwordHash == $"hashed_{password}";
    }

    private class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        public string GenerateToken(int usuarioId, string email, string rol) => $"fake_token_for_{email}_{rol}";
    }

    private class FakeSesionRepository : ISesionRepository
    {
        public Task CrearSesionAsync(Sesion sesion, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Sesion?> ObtenerPorTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult<Sesion?>(null);
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    [Fact]
    public async Task RegistrarAsync_ConDatosValidos_RetornaAuthResponseConRolTecnico()
    {
        // Arrange
        var usuarioRepo = new FakeUsuarioRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var clienteRepo = new FakeClienteRepository();
        var passwordHasher = new FakePasswordHasher();
        var jwtGenerator = new FakeJwtTokenGenerator();
        var sesionRepo = new FakeSesionRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new RegistroTecnicoService(
            usuarioRepo,
            tecnicoRepo,
            clienteRepo,
            passwordHasher,
            jwtGenerator,
            sesionRepo,
            unitOfWork);

        var request = new RegistroTecnicoRequestDto
        {
            NombreCompleto = "Roberto Gomez",
            Email = "roberto.tecnico@example.com",
            Contrasena = "Password123!",
            Telefono = "5512345678",
            TieneNegocioFisico = true,
            UbicacionEstablecimiento = "Av. Central 123",
            TelefonoNegocio = "5587654321",
            CostoCotizacion = 150.00m
        };

        // Act
        var result = await service.RegistrarAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TECNICO", result.Rol);
        Assert.Equal("roberto.tecnico@example.com", result.Email);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task RegistrarAsync_NegocioFisicoSinUbicacion_LanzaReglaNegocioInvalidaException()
    {
        // Arrange
        var service = new RegistroTecnicoService(
            new FakeUsuarioRepository(),
            new FakeTecnicoRepository(),
            new FakeClienteRepository(),
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            new FakeSesionRepository(),
            new FakeUnitOfWork());

        var request = new RegistroTecnicoRequestDto
        {
            NombreCompleto = "Roberto Gomez",
            Email = "roberto2@example.com",
            Contrasena = "Password123!",
            Telefono = "5512345678",
            TieneNegocioFisico = true,
            UbicacionEstablecimiento = null // Invalido cuando TieneNegocioFisico es true
        };

        // Act & Assert
        await Assert.ThrowsAsync<ReglaNegocioInvalidaException>(
            () => service.RegistrarAsync(request));
    }

    [Fact]
    public async Task RegistrarAsync_CorreoDuplicado_LanzaInvalidOperationException()
    {
        // Arrange
        var usuarioRepo = new FakeUsuarioRepository();
        await usuarioRepo.AgregarUsuarioAsync(new Usuario("duplicado@example.com", "hash", "TECNICO"));

        var service = new RegistroTecnicoService(
            usuarioRepo,
            new FakeTecnicoRepository(),
            new FakeClienteRepository(),
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            new FakeSesionRepository(),
            new FakeUnitOfWork());

        var request = new RegistroTecnicoRequestDto
        {
            NombreCompleto = "Roberto Gomez",
            Email = "duplicado@example.com",
            Contrasena = "Password123!",
            Telefono = "5512345678"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RegistrarAsync(request));
    }

    [Fact]
    public void Tecnico_CostoCotizacionNegativo_LanzaPrecioInvalidoException()
    {
        // Arrange
        var usuario = new Usuario("tecnico@example.com", "hash", "TECNICO");

        // Act & Assert
        Assert.Throws<PrecioInvalidoException>(() =>
            new Tecnico(
                usuario,
                "Juan Tecnico",
                new CorreoElectronico("tecnico@example.com"),
                "hash",
                new NumeroTelefono("5512345678"),
                costoCotizacion: -50.00m));
    }
}
