using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Tablon;
using TecniHub.Application.Services;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;
using TecniHub.Domain.Services;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Application.UnitTests;

public class TablonServicioTests
{
    private class FakePublicacionServicioRepository : IPublicacionServicioRepository
    {
        private readonly List<PublicacionServicio> _servicios = new();
        private int _currentId = 1;

        public Task<PublicacionServicio?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_servicios.FirstOrDefault(s => s.Id == id));
        }

        public Task<List<PublicacionServicio>> ObtenerTodosAsync(bool incluirCompletados = false, CancellationToken cancellationToken = default)
        {
            var query = _servicios.AsEnumerable();
            if (!incluirCompletados)
            {
                query = query.Where(s => s.Estado != "Completado" && s.Estado != "Finalizado");
            }
            return Task.FromResult(query.ToList());
        }

        public Task<List<PublicacionServicio>> ObtenerPorClienteIdAsync(int clienteId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_servicios.Where(s => s.ClienteId == clienteId).ToList());
        }

        public Task AgregarAsync(PublicacionServicio publicacion, CancellationToken cancellationToken = default)
        {
            // Usar reflexión para simular identity ID si es necesario
            typeof(PublicacionServicio).GetProperty("Id")?.SetValue(publicacion, _currentId++);
            _servicios.Add(publicacion);
            return Task.CompletedTask;
        }
    }

    private class FakePublicacionPiezaRepository : IPublicacionPiezaRepository
    {
        private readonly List<PublicacionPieza> _piezas = new();
        private int _currentId = 1;

        public Task<PublicacionPieza?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_piezas.FirstOrDefault(p => p.Id == id));
        }

        public Task<List<PublicacionPieza>> ObtenerTodosAsync(bool incluirCompletados = false, CancellationToken cancellationToken = default)
        {
            var query = _piezas.AsEnumerable();
            if (!incluirCompletados)
            {
                query = query.Where(p => p.EstadoDisponibilidad != "Completado" && p.EstadoDisponibilidad != "Finalizado");
            }
            return Task.FromResult(query.ToList());
        }

        public Task<List<PublicacionPieza>> ObtenerPorTecnicoIdAsync(int tecnicoId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_piezas.Where(p => p.TecnicoId == tecnicoId).ToList());
        }

        public Task AgregarAsync(PublicacionPieza publicacion, CancellationToken cancellationToken = default)
        {
            typeof(PublicacionPieza).GetProperty("Id")?.SetValue(publicacion, _currentId++);
            _piezas.Add(publicacion);
            return Task.CompletedTask;
        }
    }

    private class FakeClienteRepository : IClienteRepository
    {
        private readonly List<Cliente> _clientes = new();
        public void Agregar(Cliente c) => _clientes.Add(c);
        public Task<Cliente?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
            => Task.FromResult(_clientes.FirstOrDefault(c => c.UsuarioId == usuarioId));
        public Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(_clientes.FirstOrDefault(c => c.Correo.Valor == email));
        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(_clientes.Any(c => c.Correo.Valor == email));
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
        public Task<Tecnico?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
            => Task.FromResult(_tecnicos.FirstOrDefault(t => t.UsuarioId == usuarioId));
        public Task<Tecnico?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(_tecnicos.FirstOrDefault(t => t.Correo.Valor == email));
        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(_tecnicos.Any(t => t.Correo.Valor == email));
        public Task AgregarTecnicoAsync(Tecnico tecnico, CancellationToken cancellationToken = default)
        {
            _tecnicos.Add(tecnico);
            return Task.CompletedTask;
        }
    }

    private class FakeUsuarioRepository : IUsuarioRepository
    {
        public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AgregarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    [Fact]
    public async Task Criterio1_ClienteCreaPublicacion_RetornaBordeRojo()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var piezaRepo = new FakePublicacionPiezaRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var usuarioRepo = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var reglas = new ReglasAsignacionService();

        var usuario = new Usuario("cliente@test.com", "hash", "CLIENTE");
        typeof(Usuario).GetProperty("Id")?.SetValue(usuario, 10);
        var cliente = new Cliente(usuario, "Rodrigo", "Dominguez", new CorreoElectronico("cliente@test.com"), "hash", new NumeroTelefono("5512345678"), null);
        typeof(Cliente).GetProperty("Id")?.SetValue(cliente, 1);
        clienteRepo.Agregar(cliente);

        var service = new TablonService(servicioRepo, piezaRepo, clienteRepo, tecnicoRepo, usuarioRepo, unitOfWork, reglas);

        var request = new CrearPublicacionServicioRequestDto
        {
            Titulo = "Reparación de PC",
            TipoElectrodomestico = "Computadora",
            DescripcionFalla = "No da video al encender.",
            UbicacionAproximada = "Zona Centro"
        };

        // Act
        var result = await service.CrearPublicacionServicioAsync(10, request);

        // Assert (HU-009 Criterio 1: Borde Rojo continuo)
        Assert.NotNull(result);
        Assert.Equal("Rojo", result.CodigoColor);
        Assert.Equal("border-red-500", result.ClaseCssBorde);
        Assert.Equal("#EF4444", result.ColorHex);
        Assert.False(result.EsCompletado);
        Assert.Null(result.TextoIndicador);
    }

    [Fact]
    public async Task Criterio2_TecnicoAceptaServicio_CambiaABordeVerde()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var piezaRepo = new FakePublicacionPiezaRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var usuarioRepo = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var reglas = new ReglasAsignacionService();

        var usuarioTecnico = new Usuario("tecnico@test.com", "hash", "TECNICO");
        typeof(Usuario).GetProperty("Id")?.SetValue(usuarioTecnico, 20);
        var tecnico = new Tecnico(usuarioTecnico, "Ana P.", new CorreoElectronico("tecnico@test.com"), "hash", new NumeroTelefono("5598765432"));
        typeof(Tecnico).GetProperty("Id")?.SetValue(tecnico, 2);
        tecnicoRepo.Agregar(tecnico);

        var servicio = new PublicacionServicio(1, "Mantenimiento Consola", "Consola", "Limpieza y cambio de pasta.", "Norte");
        await servicioRepo.AgregarAsync(servicio);

        var service = new TablonService(servicioRepo, piezaRepo, clienteRepo, tecnicoRepo, usuarioRepo, unitOfWork, reglas);

        // Act
        var result = await service.AceptarServicioAsync(20, servicio.Id);

        // Assert (HU-009 Criterio 2: Borde Verde inmediatamente)
        Assert.Equal("Verde", result.CodigoColor);
        Assert.Equal("border-green-500", result.ClaseCssBorde);
        Assert.Equal("#22C55E", result.ColorHex);
        Assert.Equal(2, result.TecnicoAsignadoId);
        Assert.False(result.EsCompletado);
    }

    [Fact]
    public async Task Criterio3_TecnicoPublicaPiezaConVentaOIntercambio_GeneraBordeBicolor()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var piezaRepo = new FakePublicacionPiezaRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var usuarioRepo = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var reglas = new ReglasAsignacionService();

        var usuarioTecnico = new Usuario("tecnico@test.com", "hash", "TECNICO");
        typeof(Usuario).GetProperty("Id")?.SetValue(usuarioTecnico, 30);
        var tecnico = new Tecnico(usuarioTecnico, "FixIt Juan", new CorreoElectronico("tecnico@test.com"), "hash", new NumeroTelefono("5598765432"));
        typeof(Tecnico).GetProperty("Id")?.SetValue(tecnico, 3);
        tecnicoRepo.Agregar(tecnico);

        var service = new TablonService(servicioRepo, piezaRepo, clienteRepo, tecnicoRepo, usuarioRepo, unitOfWork, reglas);

        var request = new CrearPublicacionPiezaRequestDto
        {
            NombrePieza = "Placa Base Dañada",
            CategoriaAparato = "PC",
            CondicionPieza = "Para refacciones",
            TipoTransaccion = "VentaOIntercambio",
            DatosContacto = "5598765432",
            Precio = 300
        };

        // Act
        var result = await service.CrearPublicacionPiezaAsync(30, request);

        // Assert (HU-009 Criterio 3: Borde combinado Amarillo y Naranja)
        Assert.Equal("AmarilloNaranja", result.CodigoColor);
        Assert.Equal("border-bicolor-yellow-orange", result.ClaseCssBorde);
        Assert.False(result.EsCompletado);
    }

    [Fact]
    public async Task Criterio4_FinalizarServicio_CambiaABordeAzulYTextoCompletado()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var piezaRepo = new FakePublicacionPiezaRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var usuarioRepo = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var reglas = new ReglasAsignacionService();

        var usuarioCliente = new Usuario("cliente@test.com", "hash", "CLIENTE");
        typeof(Usuario).GetProperty("Id")?.SetValue(usuarioCliente, 10);
        var cliente = new Cliente(usuarioCliente, "Rodrigo", "Dominguez", new CorreoElectronico("cliente@test.com"), "hash", new NumeroTelefono("5512345678"), null);
        typeof(Cliente).GetProperty("Id")?.SetValue(cliente, 1);
        clienteRepo.Agregar(cliente);

        var servicio = new PublicacionServicio(1, "Reparación Refrigerador", "Refrigerador", "No enfría.", "Sur");
        servicio.AceptarPorTecnico(5); // Ponerlo en proceso
        await servicioRepo.AgregarAsync(servicio);

        var service = new TablonService(servicioRepo, piezaRepo, clienteRepo, tecnicoRepo, usuarioRepo, unitOfWork, reglas);

        // Act
        var result = await service.FinalizarServicioAsync(10, servicio.Id);

        // Assert (HU-009 Criterio 4: Borde Azul y texto Completado...)
        Assert.Equal("Azul", result.CodigoColor);
        Assert.Equal("border-blue-500", result.ClaseCssBorde);
        Assert.True(result.EsCompletado);
        Assert.Equal("Completado...", result.TextoIndicador);
    }

    [Fact]
    public async Task Criterio4_FinalizarPieza_CambiaABordeAzulMarinoYTextoCompletado()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var piezaRepo = new FakePublicacionPiezaRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var usuarioRepo = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var reglas = new ReglasAsignacionService();

        var usuarioTecnico = new Usuario("tecnico@test.com", "hash", "TECNICO");
        typeof(Usuario).GetProperty("Id")?.SetValue(usuarioTecnico, 25);
        var tecnico = new Tecnico(usuarioTecnico, "TechMaster99", new CorreoElectronico("tecnico@test.com"), "hash", new NumeroTelefono("5598765432"));
        typeof(Tecnico).GetProperty("Id")?.SetValue(tecnico, 8);
        tecnicoRepo.Agregar(tecnico);

        var pieza = new PublicacionPieza(8, "Memoria RAM 16GB", "PC", "Nueva", "5598765432", "Venta", 800);
        await piezaRepo.AgregarAsync(pieza);

        var service = new TablonService(servicioRepo, piezaRepo, clienteRepo, tecnicoRepo, usuarioRepo, unitOfWork, reglas);

        // Act
        var result = await service.FinalizarPiezaAsync(25, pieza.Id);

        // Assert (HU-009 Criterio 4: Borde Azul Marino y texto Completado...)
        Assert.Equal("AzulMarino", result.CodigoColor);
        Assert.Equal("border-blue-700", result.ClaseCssBorde);
        Assert.True(result.EsCompletado);
        Assert.Equal("Completado...", result.TextoIndicador);
    }

    [Fact]
    public async Task ReglaLimpieza_PorDefecto_OcultaCompletados()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var piezaRepo = new FakePublicacionPiezaRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var usuarioRepo = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var reglas = new ReglasAsignacionService();

        var servicioAbierto = new PublicacionServicio(1, "Servicio Abierto", "Horno", "Falla timer", "Centro");
        var servicioCompletado = new PublicacionServicio(1, "Servicio Terminado", "Lavadora", "Centrifugado", "Sur");
        servicioCompletado.AceptarPorTecnico(2);
        servicioCompletado.FinalizarServicio();

        await servicioRepo.AgregarAsync(servicioAbierto);
        await servicioRepo.AgregarAsync(servicioCompletado);

        var service = new TablonService(servicioRepo, piezaRepo, clienteRepo, tecnicoRepo, usuarioRepo, unitOfWork, reglas);

        // Act (sin incluir completados)
        var tablon = await service.ObtenerTablonAsync(new TablonFiltroDto { IncluirCompletados = false });

        // Assert: solo el abierto debe figurar
        Assert.Single(tablon);
        Assert.Equal("Servicio Abierto", tablon[0].Titulo);
    }
}
