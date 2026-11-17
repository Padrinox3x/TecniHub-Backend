using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Notificaciones;

namespace Backend_TecniHub.Controllers;

[ApiController]
[Route("api/notificaciones")]
public sealed class NotificacionesController : ControllerBase
{
    private readonly INotificacionPushService _notificacionService;

    public NotificacionesController(INotificacionPushService notificacionService)
    {
        _notificacionService = notificacionService;
    }

    /// <summary>
    /// HU-015: Registra la suscripción a notificaciones push (Service Worker / Web Push / FCM) del usuario o dispositivo.
    /// </summary>
    [HttpPost("suscribir")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Suscribir(
        [FromBody] SuscripcionPushRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var exito = await _notificacionService.SuscribirAsync(request, usuarioId, cancellationToken);
            return Ok(new { mensaje = "Suscripción a notificaciones push registrada con éxito.", exito });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// HU-015: Cancela la suscripción a notificaciones push del dispositivo.
    /// </summary>
    [HttpPost("desuscribir")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Desuscribir(
        [FromBody] DesuscripcionPushRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Endpoint))
            return BadRequest(new { mensaje = "El endpoint de suscripción es requerido." });

        var exito = await _notificacionService.DesuscribirAsync(request.Endpoint, cancellationToken);
        return Ok(new { mensaje = "Suscripción cancelada.", removido = exito });
    }

    /// <summary>
    /// HU-015: Consulta las notificaciones del usuario autenticado (con filtros opcionales de leídas/no leídas).
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<NotificacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificacionDto>>> ObtenerNotificaciones(
        [FromQuery] bool? soloNoLeidas,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (!usuarioId.HasValue)
        {
            return Unauthorized(new { mensaje = "Token no válido o no contiene identificación de usuario." });
        }

        var lista = await _notificacionService.ObtenerNotificacionesUsuarioAsync(usuarioId.Value, soloNoLeidas, cancellationToken);
        return Ok(lista);
    }

    /// <summary>
    /// HU-015: Marca una notificación como leída.
    /// </summary>
    [HttpPut("{id}/leida")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarLeida(string id, CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        var exito = await _notificacionService.MarcarComoLeidaAsync(id, usuarioId, cancellationToken);
        if (!exito)
        {
            return NotFound(new { mensaje = $"No se encontró la notificación con ID {id}." });
        }

        return Ok(new { mensaje = "Notificación marcada como leída.", leida = true });
    }

    /// <summary>
    /// HU-015: Emite una notificación push directa o simulada hacia un usuario.
    /// </summary>
    [HttpPost("enviar")]
    [Authorize]
    [ProducesResponseType(typeof(NotificacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificacionDto>> EnviarNotificacion(
        [FromBody] EnviarNotificacionRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var notificacion = await _notificacionService.EnviarNotificacionAsync(request, cancellationToken);
            return Ok(notificacion);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// HU-015: Simulador de Eventos para Testing (como se especifica en los diseños de HU-015):
    /// - TecnicoAceptoServicio (Verde)
    /// - ClienteFinalizoServicio (Azul)
    /// - NuevoMensajeChat (Amarillo)
    /// </summary>
    [HttpPost("simular")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(NotificacionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificacionDto>> SimularEvento(
        [FromBody] SimulacionEventoPushRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        var resultado = await _notificacionService.SimularEventoPushAsync(request, usuarioId, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// HU-015: Alertas de Conectividad - Endpoint de comprobación de salud y reconexión a Internet.
    /// Permite a la aplicación cliente validar si el servidor está en línea al recuperar la conexión.
    /// </summary>
    [HttpGet("ping")]
    [HttpGet("/api/conectividad/ping")]
    [HttpGet("/api/conectividad/estado")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EstadoConectividadDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EstadoConectividadDto>> Ping(CancellationToken cancellationToken)
    {
        var estado = await _notificacionService.VerificarConectividadAsync(cancellationToken);
        return Ok(estado);
    }

    private int? ObtenerUsuarioIdActual()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
