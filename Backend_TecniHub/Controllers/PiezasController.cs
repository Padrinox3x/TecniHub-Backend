using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Piezas;

namespace Backend_TecniHub.Controllers;

[ApiController]
[Route("api/piezas")]
public sealed class PiezasController : ControllerBase
{
    private readonly IPiezaService _piezaService;

    public PiezasController(IPiezaService piezaService)
    {
        _piezaService = piezaService;
    }

    /// <summary>
    /// HU-008: Busca y consulta refacciones/piezas disponibles aplicando filtros de texto, categoría, condición, modalidad o precio máximo.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<PiezaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PiezaDto>>> Buscar(
        [FromQuery] BuscarPiezasRequestDto request,
        CancellationToken cancellationToken)
    {
        var piezas = await _piezaService.BuscarPiezasAsync(request, cancellationToken);
        return Ok(piezas);
    }

    /// <summary>
    /// HU-008: Permite a un técnico publicar una refacción para venta, intercambio o ambas modalidades.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Publicar(
        [FromBody] PublicarPiezaRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var id = await _piezaService.PublicarPiezaAsync(request, usuarioId, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, new { id });
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
    /// HU-008: Obtiene el detalle de una pieza o refacción por su identificador.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PiezaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PiezaDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var pieza = await _piezaService.ObtenerPorIdAsync(id, cancellationToken);
        if (pieza is null)
        {
            return NotFound(new { mensaje = $"No se encontró la refacción con ID {id}." });
        }
        return Ok(pieza);
    }

    /// <summary>
    /// HU-008: Actualiza el estado de disponibilidad de la pieza (ej. 'Disponible', 'Completado' o 'NoDisponible').
    /// </summary>
    [HttpPut("{id:int}/disponibilidad")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarDisponibilidad(
        int id,
        [FromBody] ActualizarDisponibilidadPiezaDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            await _piezaService.CambiarDisponibilidadAsync(id, dto.EstadoDisponibilidad, usuarioId, cancellationToken);
            return Ok(new { mensaje = "Disponibilidad actualizada exitosamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    private int? ObtenerUsuarioIdActual()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
