using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Backend_TecniHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Perfil;
using TecniHub.Application.Services;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Application.UnitTests;

public class PerfilesPublicosHU012HU013Tests
{
    private class FakePublicacionServicioRepository : IPublicacionServicioRepository
    {
        public readonly List<PublicacionServicio> Servicios = new();

        public Task<PublicacionServicio?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Servicios.FirstOrDefault(s => s.Id == id));

        public Task<List<PublicacionServicio>> ObtenerTodosAsync(bool incluirCompletados = false, CancellationToken cancellationToken = default) =>
            Task.FromResult(Servicios.ToList());

        public Task<List<PublicacionServicio>> ObtenerPorClienteIdAsync(int clienteId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Servicios.Where(s => s.ClienteId == clienteId).ToList());

        public Task AgregarAsync(PublicacionServicio publicacion, CancellationToken cancellationToken = default)
        {
            if (publicacion.Id == 0)
            {
                typeof(PublicacionServicio).GetProperty("Id")?.SetValue(publicacion, Servicios.Count + 1);
            }
            Servicios.Add(publicacion);
            return Task.CompletedTask;
        }
    }

    private class FakeClienteRepository : IClienteRepository
    {
        private readonly List<Cliente> _clientes = new();

        public void Agregar(Cliente c) => _clientes.Add(c);

        public Task<Cliente?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_clientes.FirstOrDefault(c => c.Id == id));

        public Task<Cliente?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_clientes.FirstOrDefault(c => c.UsuarioId == usuarioId));

        public Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_clientes.FirstOrDefault(c => c.Correo.Valor == email));

        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_clientes.Any(c => c.Correo.Valor == email));

        public Task AgregarClienteAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            _clientes.Add(cliente);
            return Task.CompletedTask;
        }
    }

    private class FakeTecnicoRepository : ITecnicoRepository
    {
        private readonly List<Tecnico> _tecnicos = new();

        public void Agregar(Tecnico t) => _tecnicos.Add(t);

        public Task<Tecnico?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_tecnicos.FirstOrDefault(t => t.UsuarioId == usuarioId));

        public Task<Tecnico?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_tecnicos.FirstOrDefault(t => t.Correo.Valor == email));

        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_tecnicos.Any(t => t.Correo.Valor == email));

        public Task AgregarTecnicoAsync(Tecnico tecnico, CancellationToken cancellationToken = default)
        {
            _tecnicos.Add(tecnico);
            return Task.CompletedTask;
        }
    }

    // ==========================================
    // PRUEBAS HU-012: Perfil Público del Técnico
    // ==========================================
    [Fact]
    public async Task HU012_TecnicoNuevo_MuestraMensajeTecnicoNuevoSinEstrellas()
    {
        var tecnicoRepo = new FakeTecnicoRepository();
        var servicioRepo = new FakePublicacionServicioRepository();
        var clienteRepo = new FakeClienteRepository();

        var service = new PerfilPublicoService(tecnicoRepo, clienteRepo, servicioRepo);

        // Técnico 10 sin trabajos ni reseñas
        var perfil = await service.ObtenerPerfilPublicoTecnicoAsync(10);

        Assert.NotNull(perfil);
        Assert.True(perfil.EsTecnicoNuevo);
        Assert.Null(perfil.CalificacionPromedio);
        Assert.Equal("Técnico nuevo", perfil.MensajeReputacion);
        Assert.Equal(0, perfil.TotalTrabajosCompletados);
    }

    [Fact]
    public async Task HU012_ConResenas_CalculaPromedioExactoYRedondeaAUnDecimal()
    {
        var tecnicoRepo = new FakeTecnicoRepository();
        var servicioRepo = new FakePublicacionServicioRepository();
        var clienteRepo = new FakeClienteRepository();

        var srv1 = new PublicacionServicio(1, "Lavadora Mabe", "Reparación bomba", "Tula");
        srv1.AceptarPorTecnico(5);
        srv1.FinalizarServicio();
        await servicioRepo.AgregarAsync(srv1);

        var srv2 = new PublicacionServicio(1, "Refrigerador Samsung", "Carga gas", "Tula");
        srv2.AceptarPorTecnico(5);
        srv2.FinalizarServicio();
        await servicioRepo.AgregarAsync(srv2);

        var service = new PerfilPublicoService(tecnicoRepo, clienteRepo, servicioRepo);

        // Calificación 1: 5 estrellas. Calificación 2: 4 estrellas. Promedio: 4.5
        await service.AgregarResenaAsync(5, new CrearResenaRequestDto(srv1.Id, 5, "Excelente servicio, muy rápido."));
        await service.AgregarResenaAsync(5, new CrearResenaRequestDto(srv2.Id, 4, "Buen trabajo, llegó a tiempo."));

        var perfil = await service.ObtenerPerfilPublicoTecnicoAsync(5);

        Assert.NotNull(perfil);
        Assert.False(perfil.EsTecnicoNuevo);
        Assert.Equal(4.5m, perfil.CalificacionPromedio);
        Assert.Equal(2, perfil.Resenas.Count);
        Assert.Equal(5, perfil.Resenas[1].CalificacionEstrellas);
        Assert.Equal(4, perfil.Resenas[0].CalificacionEstrellas);
    }

    [Fact]
    public async Task HU012_AgregarResena_ServicioNoCompletado_LanzaExcepcion()
    {
        var tecnicoRepo = new FakeTecnicoRepository();
        var servicioRepo = new FakePublicacionServicioRepository();
        var clienteRepo = new FakeClienteRepository();

        var srvPendiente = new PublicacionServicio(1, "Estufa", "Fuga", "Tula");
        srvPendiente.AceptarPorTecnico(3); // Estado EnProceso, no completado
        await servicioRepo.AgregarAsync(srvPendiente);

        var service = new PerfilPublicoService(tecnicoRepo, clienteRepo, servicioRepo);

        await Assert.ThrowsAsync<ReglaNegocioInvalidaException>(() =>
            service.AgregarResenaAsync(3, new CrearResenaRequestDto(srvPendiente.Id, 5, "Buen trabajo")));
    }

    // ==========================================
    // PRUEBAS HU-013: Perfil Público del Cliente
    // ==========================================
    [Fact]
    public async Task HU013_PerfilCliente_EnmascaraNombreYGeneralizaUbicacion()
    {
        var tecnicoRepo = new FakeTecnicoRepository();
        var servicioRepo = new FakePublicacionServicioRepository();
        var clienteRepo = new FakeClienteRepository();

        var usuario = new Usuario("rodrigo@ejemplo.com", "hash", "CLIENTE");
        typeof(Usuario).GetProperty("Id")?.SetValue(usuario, 7);

        var cliente = new Cliente(
            usuario: usuario,
            nombre: "Rodrigo",
            apellidos: "Domínguez Crespo",
            correo: new CorreoElectronico("rodrigo@ejemplo.com"),
            contrasenaHash: "hash",
            numeroTelefono: new NumeroTelefono("7731234567"),
            direccion: "Calle 16 de Septiembre #45, Tula de Allende, Hidalgo"
        );
        clienteRepo.Agregar(cliente);

        // Cliente tiene 3 servicios solicitados
        await servicioRepo.AgregarAsync(new PublicacionServicio(7, "Horno", "Falla", "Tula"));
        await servicioRepo.AgregarAsync(new PublicacionServicio(7, "Licuadora", "No enciende", "Tula"));
        var srv3 = new PublicacionServicio(7, "Refrigerador", "Fuga", "Tula");
        srv3.AceptarPorTecnico(1);
        srv3.FinalizarServicio();
        await servicioRepo.AgregarAsync(srv3);

        var service = new PerfilPublicoService(tecnicoRepo, clienteRepo, servicioRepo);

        var perfil = await service.ObtenerPerfilPublicoClienteAsync(7);

        Assert.NotNull(perfil);
        Assert.Equal("Rodrigo D.", perfil.NombrePublico);
        Assert.Equal("Tula de Allende, Hidalgo", perfil.ZonaAproximada);
        Assert.Equal(3, perfil.ServiciosSolicitados);
        Assert.Equal(1, perfil.ServiciosCompletados);
        Assert.True(perfil.EsUsuarioFrecuente);
    }

    [Fact]
    public async Task HU013_PerfilCliente_ResuelvePorClienteIdAunqueDifieraDeUsuarioId()
    {
        var tecnicoRepo = new FakeTecnicoRepository();
        var servicioRepo = new FakePublicacionServicioRepository();
        var clienteRepo = new FakeClienteRepository();

        var usuario = new Usuario("cliente6@ejemplo.com", "hash", "CLIENTE");
        typeof(Usuario).GetProperty("Id")?.SetValue(usuario, 15);

        var cliente = new Cliente(
            usuario: usuario,
            nombre: "María",
            apellidos: "López García",
            correo: new CorreoElectronico("cliente6@ejemplo.com"),
            contrasenaHash: "hash",
            numeroTelefono: new NumeroTelefono("7731234567"),
            direccion: "Av. Hidalgo 10, Pachuca, Hidalgo"
        );
        typeof(Cliente).GetProperty("Id")?.SetValue(cliente, 6);
        clienteRepo.Agregar(cliente);

        var srv1 = new PublicacionServicio(6, "Lavadora", "No drena", "Pachuca");
        srv1.AceptarPorTecnico(1);
        srv1.FinalizarServicio();
        await servicioRepo.AgregarAsync(srv1);

        var service = new PerfilPublicoService(tecnicoRepo, clienteRepo, servicioRepo);

        var perfil = await service.ObtenerPerfilPublicoClienteAsync(6);

        Assert.NotNull(perfil);
        Assert.Equal(6, perfil.Id);
        Assert.Equal("María L.", perfil.NombrePublico);
        Assert.Equal("Pachuca, Hidalgo", perfil.ZonaAproximada);
        Assert.Equal(1, perfil.ServiciosSolicitados);
        Assert.Equal(1, perfil.ServiciosCompletados);
        Assert.Equal("100%", perfil.PorcentajeInteraccionesExitosas);
    }

    [Fact]
    public async Task HU013_PerfilCliente_Inexistente_LanzaKeyNotFoundException()
    {
        var service = new PerfilPublicoService(
            new FakeTecnicoRepository(),
            new FakeClienteRepository(),
            new FakePublicacionServicioRepository());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.ObtenerPerfilPublicoClienteAsync(999));
    }

    [Theory]
    [InlineData("/api/cliente/publico/6")]
    [InlineData("/api/Perfil/cliente/publico/6")]
    public async Task HU013_EndpointsClientePublico_Retornan200YCabecerasCors(string url)
    {
        var fakeService = new StubPerfilPublicoService(id =>
            id == 6
                ? new PerfilPublicoClienteDto(6, "María L.", "Pachuca, Hidalgo", "Noviembre 2026", 1, 1, "100%", false)
                : throw new KeyNotFoundException($"No se encontró el cliente con ID {id}."));

        using var factory = CrearFactoryConServicio(fakeService);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Origin", "http://localhost:4200");

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var corsValues));
        Assert.Contains("http://localhost:4200", corsValues);

        var dto = await response.Content.ReadFromJsonAsync<PerfilPublicoClienteDto>();
        Assert.NotNull(dto);
        Assert.Equal(6, dto!.Id);
        Assert.Equal("María L.", dto.NombrePublico);
    }

    [Theory]
    [InlineData("/api/cliente/publico/999")]
    [InlineData("/api/Perfil/cliente/publico/999")]
    public async Task HU013_EndpointsClientePublico_Inexistente_Retornan404ConCors(string url)
    {
        var fakeService = new StubPerfilPublicoService(id =>
            throw new KeyNotFoundException($"No se encontró el cliente con ID {id}."));

        using var factory = CrearFactoryConServicio(fakeService);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Origin", "http://localhost:4200");

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var corsValues));
        Assert.Contains("http://localhost:4200", corsValues);
    }

    [Fact]
    public async Task GlobalExceptionHandler_ExcepcionNoControlada_ConservaCabecerasCors()
    {
        var fakeService = new StubPerfilPublicoService(_ =>
            throw new InvalidOperationException("Fallo simulado"));

        using var factory = CrearFactoryConServicio(fakeService);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Origin", "http://localhost:4200");

        var response = await client.GetAsync("/api/cliente/publico/6");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var corsValues));
        Assert.Contains("http://localhost:4200", corsValues);
    }

    private static WebApplicationFactory<TestHostMarker> CrearFactoryConServicio(IPerfilPublicoService perfilService)
    {
        return new WebApplicationFactory<TestHostMarker>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=tecnihub_test;Username=postgres;Password=postgres",
                    ["JwtSettings:SecretKey"] = "clave-super-secreta-para-pruebas-unitarias-1234567890",
                    ["JwtSettings:Issuer"] = "TecniHubApi",
                    ["JwtSettings:Audience"] = "TecniHubClient"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPerfilPublicoService>();
                services.AddSingleton(perfilService);

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestAuth";
                    options.DefaultChallengeScheme = "TestAuth";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });
            });
        });
    }

    private sealed class StubPerfilPublicoService : IPerfilPublicoService
    {
        private readonly Func<int, PerfilPublicoClienteDto> _clienteHandler;

        public StubPerfilPublicoService(Func<int, PerfilPublicoClienteDto> clienteHandler)
        {
            _clienteHandler = clienteHandler;
        }

        public Task<PerfilPublicoTecnicoDto> ObtenerPerfilPublicoTecnicoAsync(int tecnicoId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<PerfilPublicoClienteDto> ObtenerPerfilPublicoClienteAsync(int clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult(_clienteHandler(clienteId));

        public Task<ResenaTecnicoDto> AgregarResenaAsync(int tecnicoId, CrearResenaRequestDto request, int? clienteUsuarioId = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<ResenaTecnicoDto?> ObtenerResenaPorServicioIdAsync(int servicioId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<ResenaTecnicoDto>> ObtenerResenasPorTecnicoIdAsync(int tecnicoId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "1") },
                "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestAuth");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
