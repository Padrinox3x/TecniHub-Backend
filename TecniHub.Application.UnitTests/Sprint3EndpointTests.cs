using System.Net;
using System.Net.Http.Json;
using Backend_TecniHub;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Sprint3;

namespace TecniHub.Application.UnitTests;

public sealed class Sprint3EndpointTests : IClassFixture<Sprint3ApiFactory>
{
    private readonly HttpClient _client;
    private readonly StubSprint3Repository _repository;

    public Sprint3EndpointTests(Sprint3ApiFactory factory)
    {
        _client = factory.CreateClient();
        _repository = factory.Repository;
    }

    [Fact]
    public async Task GetCategorias_Devuelve200()
    {
        var response = await _client.GetAsync("/api/servicios/categorias");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostServicioValido_Devuelve201()
    {
        var response = await _client.PostAsJsonAsync("/api/servicios", new
        {
            clienteId = Guid.NewGuid(), categoriaId = 1, tipoElectrodomestico = "Refrigerador",
            descripcionFalla = "No enfría", ubicacionAproximada = "Centro"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostServicioInvalido_Devuelve400()
    {
        var before = _repository.ServiceCreateCalls;
        var response = await _client.PostAsJsonAsync("/api/servicios", new { clienteId = Guid.Empty });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, _repository.ServiceCreateCalls);
    }

    [Fact]
    public async Task GetPiezas_Devuelve200()
    {
        var response = await _client.GetAsync("/api/piezas?texto=compresor&categoriaId=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostPiezaValida_Devuelve201()
    {
        var response = await _client.PostAsJsonAsync("/api/piezas", new
        {
            tecnicoId = Guid.NewGuid(), categoriaId = 1, nombre = "Compresor", descripcion = "En buen estado",
            condicion = "USADO_BUENO", modalidad = "VENTA", precio = 100,
            medioContacto = "correo@example.com", ubicacionEntrega = "Centro"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostPiezaConModalidadInvalida_Devuelve400()
    {
        var before = _repository.PiecePublishCalls;
        var response = await _client.PostAsJsonAsync("/api/piezas", new
        {
            tecnicoId = Guid.NewGuid(), categoriaId = 1, nombre = "Compresor", descripcion = "En buen estado",
            condicion = "USADO_BUENO", modalidad = "DONACION", medioContacto = "correo@example.com", ubicacionEntrega = "Centro"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, _repository.PiecePublishCalls);
    }

    [Fact]
    public async Task PostItinerarioValido_Devuelve200()
    {
        var response = await _client.PostAsJsonAsync("/api/itinerarios", new
        {
            servicioId = Guid.NewGuid(), tecnicoId = Guid.NewGuid(), actorId = Guid.NewGuid(),
            fecha = new DateOnly(2026, 11, 10), horaInicio = new TimeOnly(9, 0), horaFinEstimada = new TimeOnly(10, 0)
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostItinerarioTraslapado_Devuelve409()
    {
        _repository.ScheduleConflict = true;
        var response = await _client.PostAsJsonAsync("/api/itinerarios", new
        {
            servicioId = Guid.NewGuid(), tecnicoId = Guid.NewGuid(), actorId = Guid.NewGuid(),
            fecha = new DateOnly(2026, 11, 10), horaInicio = new TimeOnly(9, 0), horaFinEstimada = new TimeOnly(10, 0)
        });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostItinerarioConHorasInvalidas_Devuelve400()
    {
        var before = _repository.ScheduleCalls;
        var response = await _client.PostAsJsonAsync("/api/itinerarios", new
        {
            servicioId = Guid.NewGuid(), tecnicoId = Guid.NewGuid(), actorId = Guid.NewGuid(),
            fecha = new DateOnly(2026, 11, 10), horaInicio = new TimeOnly(10, 0), horaFinEstimada = new TimeOnly(9, 0)
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, _repository.ScheduleCalls);
    }
}

public sealed class Sprint3ApiFactory : WebApplicationFactory<TestHostMarker>
{
    public StubSprint3Repository Repository { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ISprint3Repository>();
            services.AddSingleton<ISprint3Repository>(Repository);
        });
    }
}

public sealed class StubSprint3Repository : ISprint3Repository
{
    public int ServiceCreateCalls { get; private set; }
    public int PiecePublishCalls { get; private set; }
    public int ScheduleCalls { get; private set; }
    public bool ScheduleConflict { get; set; }

    public Task<ServicioCreadoDto> CrearServicioAsync(CrearServicioRequestDto request, CancellationToken cancellationToken)
    {
        ServiceCreateCalls++;
        return Task.FromResult(new ServicioCreadoDto(Guid.NewGuid(), "SRV-TEST", "ABIERTO"));
    }

    public Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CategoriaDto>>([new CategoriaDto(1, "Refrigeración", null, null)]);

    public Task<PiezaPublicadaDto> PublicarPiezaAsync(PublicarPiezaRequestDto request, string? ipOrigen, CancellationToken cancellationToken)
    {
        PiecePublishCalls++;
        return Task.FromResult(new PiezaPublicadaDto(Guid.NewGuid()));
    }

    public Task<IReadOnlyList<PiezaDto>> BuscarPiezasAsync(BuscarPiezasRequestDto request, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<PiezaDto>>([]);

    public Task ProgramarItinerarioAsync(ProgramarItinerarioRequestDto request, string? ipOrigen, CancellationToken cancellationToken)
    {
        ScheduleCalls++;
        if (ScheduleConflict) throw new InvalidOperationException("El técnico ya tiene una cita en ese horario.");
        return Task.CompletedTask;
    }
}
