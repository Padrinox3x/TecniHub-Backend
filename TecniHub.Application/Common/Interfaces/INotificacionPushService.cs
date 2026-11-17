using TecniHub.Application.DTOs.Notificaciones;

namespace TecniHub.Application.Common.Interfaces;

public interface INotificacionPushService
{
    Task<bool> SuscribirAsync(
        SuscripcionPushRequestDto request,
        int? usuarioId = null,
        CancellationToken cancellationToken = default);

    Task<bool> DesuscribirAsync(
        string endpoint,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificacionDto>> ObtenerNotificacionesUsuarioAsync(
        int usuarioId,
        bool? soloNoLeidas = null,
        CancellationToken cancellationToken = default);

    Task<bool> MarcarComoLeidaAsync(
        string notificacionId,
        int? usuarioId = null,
        CancellationToken cancellationToken = default);

    Task<NotificacionDto> EnviarNotificacionAsync(
        EnviarNotificacionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<NotificacionDto> SimularEventoPushAsync(
        SimulacionEventoPushRequestDto request,
        int? usuarioId = null,
        CancellationToken cancellationToken = default);

    Task NotificarCambioEstadoServicioAsync(
        int servicioId,
        string nuevoEstado,
        int usuarioAccionId,
        CancellationToken cancellationToken = default);

    Task NotificarNuevoMensajeChatAsync(
        int usuarioDestinoId,
        string emisorNombre,
        string contenido,
        int? referenciaId,
        string tipoReferencia = "Chat",
        CancellationToken cancellationToken = default);

    Task<EstadoConectividadDto> VerificarConectividadAsync(
        CancellationToken cancellationToken = default);
}
