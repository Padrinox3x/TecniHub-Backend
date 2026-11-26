using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Notificaciones;
using TecniHub.Application.DTOs.Perfil;
using TecniHub.Application.DTOs.Servicios;
using TecniHub.Application.Services;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;
using TecniHub.Domain.Services;
using TecniHub.Domain.ValueObjects;
using Xunit;

namespace TecniHub.Application.UnitTests;

public class FinalizacionYNotificacionesHU014HU015Tests
{
    private class FakePublicacionServicioRepository : IPublicacionServicioRepository
    {
        public readonly List<PublicacionServicio> Servicios = new();

        public Task<PublicacionServicio?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Servicios.FirstOrDefault(s => s.Id == id));

        public Task<List<PublicacionServicio>> ObtenerTodosAsync(bool incluirCompletados = false, CancellationToken cancellationToken = default) =>
            Task.FromResult(incluirCompletados ? Servicios.ToList() : Servicios.Where(s => !string.Equals(s.Estado, "Completado", StringComparison.OrdinalIgnoreCase)).ToList());

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
            Task.FromResult(_tecnicos.FirstOrDefault());
        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
        public Task AgregarTecnicoAsync(Tecnico tecnico, CancellationToken cancellationToken = default)
        {
            _tecnicos.Add(tecnico);
            return Task.CompletedTask;
        }
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task HU014_SolicitarCierre_ServicioEnProceso_PasaAPendienteCierre_Y_NotificaAlCliente()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var unitOfWork = new FakeUnitOfWork();

        var notifService = new NotificacionPushService(servicioRepo, clienteRepo, tecnicoRepo);
        var perfilService = new PerfilPublicoService(tecnicoRepo, clienteRepo, servicioRepo, unitOfWork, notifService);
        var servicioService = new ServicioService(servicioRepo, clienteRepo, tecnicoRepo, unitOfWork, perfilService, notifService);

        var servicio = new PublicacionServicio(
            clienteId: 5,
            titulo: "Reparación de lavadora Samsung",
            tipoElectrodomestico: "Lavadoras",
            descripcionFalla: "No centrifuga",
            ubicacionAproximada: "Tula de Allende"
        );
        typeof(PublicacionServicio).GetProperty("Id")?.SetValue(servicio, 101);
        servicio.AceptarPorTecnico(tecnicoId: 1); // Pasa a 'EnProceso'
        await servicioRepo.AgregarAsync(servicio);

        // Act - Técnico solicita cierre
        var resultado = await servicioService.SolicitarCierreAsync(101);

        // Assert
        Assert.Equal("PendienteCierre", servicio.Estado);
        Assert.Equal("PendienteCierre", resultado.Estado);
        Assert.Equal("Verde", resultado.CodigoColor);
        Assert.Equal("Pendiente de confirmación", resultado.TextoIndicador);

        // Verificar notificación enviada al cliente (usuarioId = 5)
        var notificaciones = await notifService.ObtenerNotificacionesUsuarioAsync(5);
        Assert.Contains(notificaciones, n => n.Tipo == "SolicitudCierre" && n.CodigoColor == "Amarillo");
    }

    [Fact]
    public async Task HU014_ConfirmarCierreYCalificar_FinalizaServicio_GeneraResena_Y_AplicaReglaTablonAzul()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var unitOfWork = new FakeUnitOfWork();

        var notifService = new NotificacionPushService(servicioRepo, clienteRepo, tecnicoRepo);
        var perfilService = new PerfilPublicoService(tecnicoRepo, clienteRepo, servicioRepo, unitOfWork, notifService);
        var servicioService = new ServicioService(servicioRepo, clienteRepo, tecnicoRepo, unitOfWork, perfilService, notifService);

        var servicio = new PublicacionServicio(
            clienteId: 10,
            titulo: "Reparación de Refrigerador Whirlpool",
            tipoElectrodomestico: "Refrigeradores",
            descripcionFalla: "No enfría",
            ubicacionAproximada: "Pachuca"
        );
        typeof(PublicacionServicio).GetProperty("Id")?.SetValue(servicio, 202);
        servicio.AceptarPorTecnico(tecnicoId: 2);
        servicio.SolicitarCierre();
        await servicioRepo.AgregarAsync(servicio);

        var request = new ConfirmarCierreYCalificarRequestDto(
            ServicioId: 202,
            Calificacion: 5,
            Comentario: "Excelente servicio, el refrigerador quedó como nuevo."
        );

        // Act - Cliente confirma y califica
        var resultado = await servicioService.ConfirmarCierreYCalificarAsync(request);

        // Assert
        Assert.Equal("Completado", servicio.Estado);
        Assert.Equal("Completado", resultado.Estado);
        Assert.Equal("Azul", resultado.CodigoColor);
        Assert.Equal("Completado...", resultado.TextoIndicador);
        Assert.NotNull(resultado.ResenaId);

        // Verificar evaluador de color de tablón HU-006 / HU-014
        var visual = TablonColorEvaluator.EvaluarServicio(servicio);
        Assert.Equal("Azul", visual.CodigoColor);
        Assert.True(visual.EsCompletado);
        Assert.Equal("Completado...", visual.TextoIndicador);

        // Verificar que la reseña quedó registrada en el perfil del técnico
        var resena = await perfilService.ObtenerResenaPorServicioIdAsync(202);
        Assert.NotNull(resena);
        Assert.Equal((short)5, resena.CalificacionEstrellas);
    }

    [Fact]
    public async Task HU014_ConfirmarCierre_CalificacionInvalida_LanzaExcepcion()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var unitOfWork = new FakeUnitOfWork();

        var notifService = new NotificacionPushService(servicioRepo, clienteRepo, tecnicoRepo);
        var perfilService = new PerfilPublicoService(tecnicoRepo, clienteRepo, servicioRepo, unitOfWork, notifService);
        var servicioService = new ServicioService(servicioRepo, clienteRepo, tecnicoRepo, unitOfWork, perfilService, notifService);

        var servicio = new PublicacionServicio(5, "Microondas", "Falla fusible", "Centro");
        typeof(PublicacionServicio).GetProperty("Id")?.SetValue(servicio, 303);
        servicio.AceptarPorTecnico(1);
        await servicioRepo.AgregarAsync(servicio);

        var request = new ConfirmarCierreYCalificarRequestDto(303, Calificacion: 6); // Inválido (debe ser 1 a 5)

        // Act & Assert
        await Assert.ThrowsAsync<ReglaNegocioInvalidaException>(() =>
            servicioService.ConfirmarCierreYCalificarAsync(request));
    }

    [Fact]
    public async Task HU014_UnicidadDeResena_NoPermiteCalificarDosVecesElMismoServicio()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var unitOfWork = new FakeUnitOfWork();

        var notifService = new NotificacionPushService(servicioRepo, clienteRepo, tecnicoRepo);
        var perfilService = new PerfilPublicoService(tecnicoRepo, clienteRepo, servicioRepo, unitOfWork, notifService);

        var servicio = new PublicacionServicio(8, "Estufa", "Fuga quemador", "Tula");
        typeof(PublicacionServicio).GetProperty("Id")?.SetValue(servicio, 404);
        servicio.AceptarPorTecnico(3);
        servicio.FinalizarServicio();
        await servicioRepo.AgregarAsync(servicio);

        // Primera calificación: debe ser exitosa
        await perfilService.AgregarResenaAsync(3, new CrearResenaRequestDto(404, 5, "Primera reseña exitosa"));

        // Segunda calificación para el mismo servicio: debe fallar por unicidad
        var ex = await Assert.ThrowsAsync<ReglaNegocioInvalidaException>(() =>
            perfilService.AgregarResenaAsync(3, new CrearResenaRequestDto(404, 4, "Intento duplicado")));

        Assert.Contains("Solo se puede emitir una calificación y reseña por cada servicio completado", ex.Message);
    }

    [Fact]
    public async Task HU015_PushNotifications_Suscribir_Desuscribir_Y_MarcarLeida()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var notifService = new NotificacionPushService(servicioRepo, clienteRepo, tecnicoRepo);

        // 1. Suscripción Push
        var subRequest = new SuscripcionPushRequestDto(
            Endpoint: "https://fcm.googleapis.com/fcm/send/fake-endpoint-12345",
            P256dh: "key_p256dh",
            Auth: "auth_secret",
            Plataforma: "Web",
            UsuarioId: 77
        );
        var suscrito = await notifService.SuscribirAsync(subRequest, usuarioId: 77);
        Assert.True(suscrito);

        // 2. Enviar notificación
        var notif = await notifService.EnviarNotificacionAsync(new EnviarNotificacionRequestDto(
            UsuarioId: 77,
            Titulo: "🟢 Servicio Aceptado",
            Mensaje: "El técnico @TechMaster99 ha aceptado tu reparación de consola.",
            Tipo: "ServicioAceptado",
            CodigoColor: "Verde"
        ));

        Assert.NotNull(notif.Id);
        Assert.False(notif.Leida);

        // 3. Consultar notificaciones
        var lista = await notifService.ObtenerNotificacionesUsuarioAsync(77);
        Assert.Single(lista);
        Assert.Equal("ServicioAceptado", lista[0].Tipo);

        // 4. Marcar como leída
        var marcada = await notifService.MarcarComoLeidaAsync(notif.Id, usuarioId: 77);
        Assert.True(marcada);

        var noLeidas = await notifService.ObtenerNotificacionesUsuarioAsync(77, soloNoLeidas: true);
        Assert.Empty(noLeidas);

        // 5. Desuscribir
        var desuscrito = await notifService.DesuscribirAsync(subRequest.Endpoint);
        Assert.True(desuscrito);
    }

    [Fact]
    public async Task HU015_SimuladorEventosTesting_GeneraNotificacionesEspecificadasEnDiseno()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var notifService = new NotificacionPushService(servicioRepo, clienteRepo, tecnicoRepo);

        // 1. Simular: Técnico aceptó el servicio (Verde)
        var n1 = await notifService.SimularEventoPushAsync(new SimulacionEventoPushRequestDto(
            TipoEvento: "TecnicoAceptoServicio",
            NombreContraparte: "@TechMaster99",
            TituloItem: "reparación de consola"
        ), usuarioId: 10);

        Assert.Equal("Servicio Aceptado", n1.Titulo);
        Assert.Equal("Verde", n1.CodigoColor);
        Assert.Equal("#22C55E", n1.ColorHex);

        // 2. Simular: Cliente finalizó el servicio (Azul)
        var n2 = await notifService.SimularEventoPushAsync(new SimulacionEventoPushRequestDto(
            TipoEvento: "ClienteFinalizoServicio",
            NombreContraparte: "Rodrigo D."
        ), usuarioId: 10);

        Assert.Equal("Trabajo Completado", n2.Titulo);
        Assert.Equal("Azul", n2.CodigoColor);
        Assert.Equal("#3B82F6", n2.ColorHex);

        // 3. Simular: Nuevo mensaje en el chat (Amarillo)
        var n3 = await notifService.SimularEventoPushAsync(new SimulacionEventoPushRequestDto(
            TipoEvento: "NuevoMensajeChat",
            NombreContraparte: "FixIt_Juan",
            TituloItem: "Memoria RAM"
        ), usuarioId: 10);

        Assert.Equal("Nuevo Mensaje", n3.Titulo);
        Assert.Equal("Amarillo", n3.CodigoColor);
        Assert.Equal("#EAB308", n3.ColorHex);
    }

    [Fact]
    public async Task HU015_Conectividad_PingRetornaEstadoOnlineConTimestamp()
    {
        // Arrange
        var servicioRepo = new FakePublicacionServicioRepository();
        var clienteRepo = new FakeClienteRepository();
        var tecnicoRepo = new FakeTecnicoRepository();
        var notifService = new NotificacionPushService(servicioRepo, clienteRepo, tecnicoRepo);

        // Act
        var estado = await notifService.VerificarConectividadAsync();

        // Assert
        Assert.True(estado.Online);
        Assert.False(string.IsNullOrWhiteSpace(estado.Mensaje));
        Assert.True(estado.Timestamp <= DateTime.UtcNow);
    }
}
