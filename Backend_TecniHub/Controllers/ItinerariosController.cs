using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Itinerarios;

namespace Backend_TecniHub.Controllers;

[ApiController]
[Route("api/itinerarios")]
public sealed class ItinerariosController : ControllerBase
{
    private readonly IItinerarioService _itinerarioService;

    public ItinerariosController(IItinerarioService itinerarioService)
    {
        _itinerarioService = itinerarioService;
    }

    /// <summary>
    /// HU-010: Agrega un servicio aceptado al itinerario o agenda personal del técnico asignándole fecha y horario.
    /// Valida estado del servicio y advierte si existe conflicto de horario cercano.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ItinerarioProgramadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ItinerarioProgramadoDto>> Programar(
        [FromBody] ProgramarItinerarioRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var result = await _itinerarioService.ProgramarItinerarioAsync(request, usuarioId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// HU-010: Consulta los servicios agendados en el itinerario del técnico ordenados cronológicamente desde la fecha y hora más próxima.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ItinerarioItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ItinerarioItemDto>>> ObtenerItinerario(
        [FromQuery] int? tecnicoId,
        [FromQuery] bool incluirCompletados = false,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        var itinerario = await _itinerarioService.ObtenerItinerarioAsync(tecnicoId, usuarioId, incluirCompletados, cancellationToken);
        return Ok(itinerario);
    }

    /// <summary>
    /// HU-010: Marca el servicio agendado como finalizado, retirándolo del itinerario activo y moviéndolo al historial.
    /// </summary>
    [HttpPut("servicios/{id:int}/finalizar")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FinalizarServicio(int id, CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            await _itinerarioService.FinalizarServicioAgendadoAsync(id, usuarioId, cancellationToken);
            return Ok(new { mensaje = "Servicio agendado finalizado con éxito." });
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
