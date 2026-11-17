using TecniHub.Application.DTOs.Chat;

namespace TecniHub.Application.Common.Interfaces;

public interface IChatService
{
    Task<ConversacionResumenDto> ObtenerOIniciarConversacionServicioAsync(
        int servicioId,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MensajeDto>> ObtenerHistorialMensajesServicioAsync(
        int servicioId,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);

    Task<MensajeDto> EnviarMensajeServicioAsync(
        int servicioId,
        EnviarMensajeRequestDto request,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);

    Task<ConversacionResumenDto> ObtenerOIniciarConversacionPiezaAsync(
        int piezaId,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MensajeDto>> ObtenerHistorialMensajesPiezaAsync(
        int piezaId,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);

    Task<MensajeDto> EnviarMensajePiezaAsync(
        int piezaId,
        EnviarMensajeRequestDto request,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConversacionResumenDto>> ObtenerMisConversacionesAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);
}
