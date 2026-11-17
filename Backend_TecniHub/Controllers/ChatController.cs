using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Chat;
using TecniHub.Domain.Exceptions;

namespace Backend_TecniHub.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    /// <summary>
    /// HU-011: Abre o consulta la sala de conversación privada vinculada a un servicio de reparación aceptado.
    /// Valida condición de apertura (debe estar aceptado por un técnico).
    /// </summary>
    [HttpGet("servicios/{servicioId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ConversacionResumenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversacionResumenDto>> ObtenerConversacionServicio(
        int servicioId,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var conversacion = await _chatService.ObtenerOIniciarConversacionServicioAsync(
                servicioId,
                usuarioId,
                cancellationToken);

            return Ok(conversacion);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// HU-011: Carga el historial completo de mensajes enviados y recibidos ligados a un servicio específico de forma secuencial y cronológica.
    /// </summary>
    [HttpGet("servicios/{servicioId:int}/mensajes")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<MensajeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MensajeDto>>> ObtenerMensajesServicio(
        int servicioId,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var mensajes = await _chatService.ObtenerHistorialMensajesServicioAsync(
                servicioId,
                usuarioId,
                cancellationToken);

            return Ok(mensajes);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// HU-011: Envía un mensaje de texto dentro de la conversación del servicio. Registra hora, emisor y contenido.
    /// </summary>
    [HttpPost("servicios/{servicioId:int}/mensajes")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MensajeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MensajeDto>> EnviarMensajeServicio(
        int servicioId,
        [FromBody] EnviarMensajeRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var mensaje = await _chatService.EnviarMensajeServicioAsync(
                servicioId,
                request,
                usuarioId,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, mensaje);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// HU-011: Abre o consulta la sala de conversación vinculada al interés mutuo en una refacción/pieza.
    /// </summary>
    [HttpGet("piezas/{piezaId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ConversacionResumenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversacionResumenDto>> ObtenerConversacionPieza(
        int piezaId,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var conversacion = await _chatService.ObtenerOIniciarConversacionPiezaAsync(
                piezaId,
                usuarioId,
                cancellationToken);

            return Ok(conversacion);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// HU-011: Consulta el historial de mensajes de la conversación de una refacción/pieza.
    /// </summary>
    [HttpGet("piezas/{piezaId:int}/mensajes")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<MensajeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MensajeDto>>> ObtenerMensajesPieza(
        int piezaId,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var mensajes = await _chatService.ObtenerHistorialMensajesPiezaAsync(
                piezaId,
                usuarioId,
                cancellationToken);

            return Ok(mensajes);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// HU-011: Envía un mensaje de texto sobre una refacción/pieza publicada.
    /// </summary>
    [HttpPost("piezas/{piezaId:int}/mensajes")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MensajeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MensajeDto>> EnviarMensajePieza(
        int piezaId,
        [FromBody] EnviarMensajeRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var mensaje = await _chatService.EnviarMensajePiezaAsync(
                piezaId,
                request,
                usuarioId,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, mensaje);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// HU-011: Consulta todas las conversaciones activas del usuario autenticado (para la sección "Mis mensajes").
    /// </summary>
    [HttpGet("mis-conversaciones")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ConversacionResumenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ConversacionResumenDto>>> ObtenerMisConversaciones(CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Se requiere token válido." });
        }

        var conversaciones = await _chatService.ObtenerMisConversacionesAsync(usuarioId.Value, cancellationToken);
        return Ok(conversaciones);
    }

    private int? ObtenerUsuarioIdActual()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
