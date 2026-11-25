using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Itinerarios;
using TecniHub.Application.DTOs.Piezas;
using TecniHub.Application.DTOs.Servicios;
using TecniHub.Application.Services;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Application.UnitTests;

public class HistoriasUsuarioSprint3Tests
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

    private class FakePublicacionPiezaRepository : IPublicacionPiezaRepository
    {
        public readonly List<PublicacionPieza> Piezas = new();

        public Task<PublicacionPieza?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Piezas.FirstOrDefault(p => p.Id == id));

        public Task<List<PublicacionPieza>> ObtenerTodosAsync(bool incluirCompletados = false, CancellationToken cancellationToken = default) =>
            Task.FromResult(Piezas.ToList());

        public Task<List<PublicacionPieza>> ObtenerPorTecnicoIdAsync(int tecnicoId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Piezas.Where(p => p.TecnicoId == tecnicoId).ToList());

        public Task AgregarAsync(PublicacionPieza publicacion, CancellationToken cancellationToken = default)
        {
            if (publicacion.Id == 0)
            {
                typeof(PublicacionPieza).GetProperty("Id")?.SetValue(publicacion, Piezas.Count + 1);
            }
            Piezas.Add(publicacion);
            return Task.CompletedTask;
        }
    }

    private class FakeClienteRepository : IClienteRepository
    {
        public Task<Cliente?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Cliente?>(null);
        public Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult<Cliente?>(null);
        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
        public Task AgregarClienteAsync(Cliente cliente, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private class FakeTecnicoRepository : ITecnicoRepository
    {
        public Task<Tecnico?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Tecnico?>(null);
        public Task<Tecnico?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult<Tecnico?>(null);
        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
        public Task AgregarTecnicoAsync(Tecnico tecnico, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<Tecnico?> ObtenerPorIdAsync(int tecnicoId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Tecnico?>(null);
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    // ==========================================
    // PRUEBAS HU-007: Formulario Servicio
    // ==========================================
    [Fact]
    public async Task HU007_ObtenerCategorias_RetornaCategoriasDisponibles()
    {
        var servicioRepo = new FakePublicacionServicioRepository();
        var service = new ServicioService(servicioRepo, new FakeClienteRepository(), new FakeTecnicoRepository(), new FakeUnitOfWork());

        var categorias = await service.ObtenerCategoriasAsync();

        Assert.NotEmpty(categorias);
        Assert.Contains(categorias, c => c.Nombre.Contains("Refrigeradores"));
        Assert.Contains(categorias, c => c.Nombre.Contains("Lavadoras"));
    }

    [Fact]
    public async Task HU007_CrearServicio_ConDatosValidos_PublicaEnTablonYRetornaFolio()
    {
        var servicioRepo = new FakePublicacionServicioRepository();
        var service = new ServicioService(servicioRepo, new FakeClienteRepository(), new FakeTecnicoRepository(), new FakeUnitOfWork());

        var request = new CrearServicioRequestDto(
            ClienteId: "1",
            CategoriaId: 1,
            TipoElectrodomestico: "Refrigerador",
            Marca: "Mabe",
            Modelo: "RMA1130X",
            DescripcionFalla: "No enfría en la parte inferior",
            UbicacionAproximada: "Centro de Tula",
            Municipio: "Tula de Allende",
            FechaSolicitud: DateOnly.FromDateTime(DateTime.Today),
            HorarioPreferido: "Por la mañana"
        );

        var result = await service.CrearServicioAsync(request);

        Assert.NotNull(result);
        Assert.StartsWith("SRV-", result.CodigoFolio);
        Assert.Equal("Pendiente", result.Estado);
        Assert.Single(servicioRepo.Servicios);
    }

    [Fact]
    public async Task HU007_CrearServicio_SinTipoElectrodomestico_LanzaExcepcion()
    {
        var servicioRepo = new FakePublicacionServicioRepository();
        var service = new ServicioService(servicioRepo, new FakeClienteRepository(), new FakeTecnicoRepository(), new FakeUnitOfWork());

        var request = new CrearServicioRequestDto(
            ClienteId: "1",
            CategoriaId: 1,
            TipoElectrodomestico: "",
            Marca: "Mabe",
            Modelo: "RMA1130X",
            DescripcionFalla: "Falla",
            UbicacionAproximada: "Tula",
            Municipio: null,
            FechaSolicitud: null,
            HorarioPreferido: null
        );

        await Assert.ThrowsAsync<ReglaNegocioInvalidaException>(() => service.CrearServicioAsync(request));
    }

    // ==========================================
    // PRUEBAS HU-008: Formulario de Piezas
    // ==========================================
    [Fact]
    public async Task HU008_PublicarPieza_ModalidadAmbos_RegistraCorrectamente()
    {
        var piezaRepo = new FakePublicacionPiezaRepository();
        var service = new PiezaService(piezaRepo, new FakeTecnicoRepository(), new FakeUnitOfWork());

        var request = new PublicarPiezaRequestDto(
            TecnicoId: "1",
            CategoriaId: 1,
            Nombre: "Compresor 1/4 HP",
            Descripcion: "Compresor Embraco para refrigerador R134a",
            Condicion: "NUEVO",
            Modalidad: "VENTA_E_INTERCAMBIO",
            Precio: 1200.50m,
            PiezaIntercambioDeseada: "Tarjeta control Mabe",
            MedioContacto: "tecnico@ejemplo.com",
            UbicacionEntrega: "Tula Centro"
        );

        var id = await service.PublicarPiezaAsync(request);

        Assert.NotNull(id);
        Assert.Single(piezaRepo.Piezas);
        var piezaCreada = piezaRepo.Piezas.First();
        Assert.Equal("Compresor 1/4 HP", piezaCreada.NombrePieza);
        Assert.Equal("Ambos", piezaCreada.TipoTransaccion);
        Assert.Equal("Disponible", piezaCreada.EstadoDisponibilidad);
    }

    [Fact]
    public async Task HU008_BuscarPiezas_FiltraPorTextoYDisponibilidad()
    {
        var piezaRepo = new FakePublicacionPiezaRepository();
        var service = new PiezaService(piezaRepo, new FakeTecnicoRepository(), new FakeUnitOfWork());

        await service.PublicarPiezaAsync(new PublicarPiezaRequestDto(
            TecnicoId: "1", CategoriaId: 1, Nombre: "Termostato Danfoss",
            Descripcion: "Control de temperatura", Condicion: "NUEVO", Modalidad: "VENTA",
            Precio: 350m, PiezaIntercambioDeseada: null, MedioContacto: "7731234567", UbicacionEntrega: "Tula"
        ));

        await service.PublicarPiezaAsync(new PublicarPiezaRequestDto(
            TecnicoId: "1", CategoriaId: 2, Nombre: "Bomba desagüe lavadora Whirlpool",
            Descripcion: "Bomba original", Condicion: "USADO_EXCELENTE", Modalidad: "VENTA",
            Precio: 450m, PiezaIntercambioDeseada: null, MedioContacto: "7731234567", UbicacionEntrega: "Tula"
        ));

        var resultados = await service.BuscarPiezasAsync(new BuscarPiezasRequestDto(Texto: "Termostato"));

        Assert.Single(resultados);
        Assert.Equal("Termostato Danfoss", resultados.First().Nombre);
    }

    // ==========================================
    // PRUEBAS HU-010: Agenda e Itinerario Técnico
    // ==========================================
    [Fact]
    public async Task HU010_ProgramarItinerario_ServicioAceptado_GuardaYOrdenaCronologicamente()
    {
        var servicioRepo = new FakePublicacionServicioRepository();
        var srv = new PublicacionServicio(1, "Lavadora Whirlpool", "No centrifuga", "Centro");
        srv.AceptarPorTecnico(1);
        await servicioRepo.AgregarAsync(srv);

        var service = new ItinerarioService(servicioRepo, new FakeTecnicoRepository(), new FakeUnitOfWork());

        var request = new ProgramarItinerarioRequestDto(
            ServicioId: srv.Id.ToString(),
            TecnicoId: "1",
            ActorId: "1",
            Fecha: DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
            HoraInicio: "09:00:00",
            HoraFinEstimada: "11:00:00",
            Notas: "Llevar multímetro y juego de llaves"
        );

        var resultado = await service.ProgramarItinerarioAsync(request);

        Assert.NotNull(resultado);
        Assert.Equal("Programado", resultado.Estado);

        var itinerario = await service.ObtenerItinerarioAsync(tecnicoId: 1);
        Assert.Contains(itinerario, item => item.ServicioId == srv.Id);
    }

    [Fact]
    public async Task HU010_ProgramarItinerario_ConHorarioEmpalmado_GeneraAdvertenciaCercana()
    {
        var servicioRepo = new FakePublicacionServicioRepository();
        var srv1 = new PublicacionServicio(1, "Servicio 1", "Falla 1", "Centro");
        srv1.AceptarPorTecnico(1);
        await servicioRepo.AgregarAsync(srv1);

        var srv2 = new PublicacionServicio(1, "Servicio 2", "Falla 2", "Centro");
        srv2.AceptarPorTecnico(1);
        await servicioRepo.AgregarAsync(srv2);

        var service = new ItinerarioService(servicioRepo, new FakeTecnicoRepository(), new FakeUnitOfWork());
        var manana = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");

        await service.ProgramarItinerarioAsync(new ProgramarItinerarioRequestDto(
            ServicioId: srv1.Id.ToString(),
            TecnicoId: "1",
            ActorId: "1",
            Fecha: manana,
            HoraInicio: "10:00:00",
            HoraFinEstimada: "12:00:00"
        ));

        // Intento de programar segundo servicio en horario empalmado/cercano (11:00 a 13:00)
        var segundo = await service.ProgramarItinerarioAsync(new ProgramarItinerarioRequestDto(
            ServicioId: srv2.Id.ToString(),
            TecnicoId: "1",
            ActorId: "1",
            Fecha: manana,
            HoraInicio: "11:00:00",
            HoraFinEstimada: "13:00:00"
        ));

        Assert.NotNull(segundo.Advertencia);
        Assert.Contains("cerca de esta hora", segundo.Advertencia);
    }

    [Fact]
    public async Task HU010_FinalizarServicio_MarcaCompletadoEnItinerarioYServicio()
    {
        var servicioRepo = new FakePublicacionServicioRepository();
        var srv = new PublicacionServicio(1, "Horno", "No calienta", "Tula");
        srv.AceptarPorTecnico(1);
        await servicioRepo.AgregarAsync(srv);

        var service = new ItinerarioService(servicioRepo, new FakeTecnicoRepository(), new FakeUnitOfWork());

        await service.ProgramarItinerarioAsync(new ProgramarItinerarioRequestDto(
            ServicioId: srv.Id.ToString(),
            TecnicoId: "1",
            ActorId: "1",
            Fecha: DateTime.Today.ToString("yyyy-MM-dd"),
            HoraInicio: "14:00:00",
            HoraFinEstimada: "15:30:00"
        ));

        await service.FinalizarServicioAgendadoAsync(srv.Id);

        Assert.Equal("Completado", srv.Estado);
        var activos = await service.ObtenerItinerarioAsync(tecnicoId: 1, incluirCompletados: false);
        Assert.DoesNotContain(activos, c => c.ServicioId == srv.Id);
    }
}
