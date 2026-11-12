using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Tablon;
using TecniHub.Domain.Exceptions;

namespace Backend_TecniHub.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/tablon")]
public class TablonServicioController : ControllerBase
{
    private readonly ITablonService _tablonService;

    public TablonServicioController(ITablonService tablonService)
    {
        _tablonService = tablonService;
    }

    /// <summary>
    /// Consulta pública de publicaciones en el tablón (Servicios y Piezas) con código de colores.
    /// Por defecto oculta publicaciones completadas según la regla de limpieza de HU-009.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<TablonItemDto>>> ObtenerTablon(
        [FromQuery] TablonFiltroDto? filtro,
        CancellationToken cancellationToken)
    {
        var items = await _tablonService.ObtenerTablonAsync(filtro, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Consulta el detalle de una solicitud de servicio específica.
    /// </summary>
    [HttpGet("servicios/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<TablonItemDto>> ObtenerServicioPorId(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _tablonService.ObtenerPublicacionServicioPorIdAsync(id, cancellationToken);
            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Consulta el detalle de una publicación de refacción/pieza específica.
    /// </summary>
    [HttpGet("piezas/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<TablonItemDto>> ObtenerPiezaPorId(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _tablonService.ObtenerPublicacionPiezaPorIdAsync(id, cancellationToken);
            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Publica una solicitud de servicio de reparación en el tablón (Genera borde Rojo continuo).
    /// Permitido únicamente para usuarios con perfil Cliente.
    /// </summary>
    [HttpPost("servicios")]
    [Authorize]
    public async Task<ActionResult<TablonItemDto>> CrearPublicacionServicio(
        [FromBody] CrearPublicacionServicioRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Token no válido o no contiene identificación de usuario." });
        }

        try
        {
            var item = await _tablonService.CrearPublicacionServicioAsync(
                usuarioId.Value,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(ObtenerServicioPorId),
                new { id = item.Id },
                item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
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
    /// Publica una refacción o pieza en el tablón (Genera borde Amarillo, Naranja o Bicolor).
    /// Permitido únicamente para técnicos registrados.
    /// </summary>
    [HttpPost("piezas")]
    [Authorize]
    public async Task<ActionResult<TablonItemDto>> CrearPublicacionPieza(
        [FromBody] CrearPublicacionPiezaRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Token no válido o no contiene identificación de usuario." });
        }

        try
        {
            var item = await _tablonService.CrearPublicacionPiezaAsync(
                usuarioId.Value,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(ObtenerPiezaPorId),
                new { id = item.Id },
                item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
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
    /// Un técnico acepta una solicitud de servicio (El borde cambia inmediatamente a color Verde).
    /// </summary>
    [HttpPut("servicios/{id:int}/aceptar")]
    [Authorize]
    public async Task<ActionResult<TablonItemDto>> AceptarServicio(
        int id,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Token no válido o no contiene identificación de usuario." });
        }

        try
        {
            var item = await _tablonService.AceptarServicioAsync(
                usuarioId.Value,
                id,
                cancellationToken);

            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
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
    /// Marca un servicio de reparación como finalizado (El borde cambia a Azul y muestra "Completado...").
    /// </summary>
    [HttpPut("servicios/{id:int}/finalizar")]
    [Authorize]
    public async Task<ActionResult<TablonItemDto>> FinalizarServicio(
        int id,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Token no válido o no contiene identificación de usuario." });
        }

        try
        {
            var item = await _tablonService.FinalizarServicioAsync(
                usuarioId.Value,
                id,
                cancellationToken);

            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
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
    /// Marca una venta o intercambio de refacción como completado (El borde cambia a Azul Marino y muestra "Completado...").
    /// </summary>
    [HttpPut("piezas/{id:int}/finalizar")]
    [Authorize]
    public async Task<ActionResult<TablonItemDto>> FinalizarPieza(
        int id,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Token no válido o no contiene identificación de usuario." });
        }

        try
        {
            var item = await _tablonService.FinalizarPiezaAsync(
                usuarioId.Value,
                id,
                cancellationToken);

            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
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

    private int? ObtenerUsuarioIdActual()
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value;

        return int.TryParse(claimValue, out var id) ? id : null;
    }
}
