using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Chat;
using TecniHub.Application.Services;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;

namespace TecniHub.Application.UnitTests;

public class ChatInternoHU011Tests
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

    private class FakeUsuarioRepository : IUsuarioRepository
    {
        public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Usuario?>(null);
        public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult<Usuario?>(null);
        public Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
        public Task AgregarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    [Fact]
    public async Task HU011_CondicionApertura_ServicioNoAceptado_RechazaAperturaChat()
    {
        var servicioRepo = new FakePublicacionServicioRepository();
        var srvPendiente = new PublicacionServicio(1, "Lavadora", "Falla motor", "Pachuca");
        await servicioRepo.AgregarAsync(srvPendiente);

        var chatService = new ChatService(
            servicioRepo,
            new FakePublicacionPiezaRepository(),
            new FakeClienteRepository(),
            new FakeTecnicoRepository(),
            new FakeUsuarioRepository()
        );

        // Intento de abrir chat para un servicio que sigue pendiente (sin técnico aceptado)
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            chatService.ObtenerOIniciarConversacionServicioAsync(srvPendiente.Id));
    }

    [Fact]
    public async Task HU011_CondicionApertura_ServicioAceptado_PermiteAperturaChat()
    {
        var servicioRepo = new FakePublicacionServicioRepository();
        var srvAceptado = new PublicacionServicio(1, "Refrigerador LG", "No enfría", "Tula");
        srvAceptado.AceptarPorTecnico(2);
        await servicioRepo.AgregarAsync(srvAceptado);

        var chatService = new ChatService(
            servicioRepo,
            new FakePublicacionPiezaRepository(),
            new FakeClienteRepository(),
            new FakeTecnicoRepository(),
            new FakeUsuarioRepository()
        );

        var conversacion = await chatService.ObtenerOIniciarConversacionServicioAsync(srvAceptado.Id);

        Assert.NotNull(conversacion);
        Assert.Equal("SERVICIO", conversacion.TipoContexto);
        Assert.Equal(srvAceptado.Id, conversacion.ContextoId);
        Assert.Equal(1, conversacion.ClienteUsuarioId);
        Assert.Equal(2, conversacion.TecnicoUsuarioId);
    }

    [Fact]
    public async Task HU011_PrivacidadSeguridad_UsuarioAjeno_RechazaAcceso()
    {
        var servicioRepo = new FakePublicacionServicioRepository();
        var srv = new PublicacionServicio(clienteId: 10, "Estufa", "Fuga de gas", "Tula");
        srv.AceptarPorTecnico(tecnicoId: 20);
        await servicioRepo.AgregarAsync(srv);

        var chatService = new ChatService(
            servicioRepo,
            new FakePublicacionPiezaRepository(),
            new FakeClienteRepository(),
            new FakeTecnicoRepository(),
            new FakeUsuarioRepository()
        );

        // Usuario 99 es un tercero ajeno a la orden
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            chatService.ObtenerOIniciarConversacionServicioAsync(srv.Id, usuarioIdAutenticado: 99));
    }

    [Fact]
    public async Task HU011_EnviarMensajeYConsultarHistorial_GuardaSecuenciaConHora()
    {
        var servicioRepo = new FakePublicacionServicioRepository();
        var srv = new PublicacionServicio(clienteId: 1, "Microondas", "Chispas al calentar", "Tula");
        srv.AceptarPorTecnico(tecnicoId: 2);
        await servicioRepo.AgregarAsync(srv);

        var chatService = new ChatService(
            servicioRepo,
            new FakePublicacionPiezaRepository(),
            new FakeClienteRepository(),
            new FakeTecnicoRepository(),
            new FakeUsuarioRepository()
        );

        // Técnico envía mensaje
        var msg1 = await chatService.EnviarMensajeServicioAsync(
            srv.Id,
            new EnviarMensajeRequestDto("¡Hola! ¿A qué hora te queda mejor que pase a revisarlo?", RemitenteUsuarioId: 2),
            usuarioIdAutenticado: 2
        );

        // Cliente responde
        var msg2 = await chatService.EnviarMensajeServicioAsync(
            srv.Id,
            new EnviarMensajeRequestDto("Hola, ¿podría ser este viernes por la tarde? Como a las 4:00 PM.", RemitenteUsuarioId: 1),
            usuarioIdAutenticado: 1
        );

        Assert.NotNull(msg1);
        Assert.NotNull(msg2);
        Assert.True(msg1.Id < msg2.Id);
        Assert.Contains("4:00 PM", msg2.Contenido);

        // Criterio 3: Usuario regresa después y consulta el historial completo
        var historial = await chatService.ObtenerHistorialMensajesServicioAsync(srv.Id, usuarioIdAutenticado: 1);

        Assert.Equal(2, historial.Count);
        Assert.Equal(msg1.Contenido, historial[0].Contenido);
        Assert.False(historial[0].EsMio); // Lo mandó el técnico
        Assert.Equal(msg2.Contenido, historial[1].Contenido);
        Assert.True(historial[1].EsMio);  // Lo mandó el cliente
    }
}
