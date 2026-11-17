using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Sprint3;

namespace Backend_TecniHub.Controllers;

[ApiController]
[Route("api/itinerarios")]
public sealed class ItinerariosController(ISprint3Service service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ItinerarioProgramadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ItinerarioProgramadoDto>> Programar([FromBody] ProgramarItinerarioRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ProgramarItinerarioAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { mensaje = ex.Message }); }
        catch (PostgresException ex) when (ex.SqlState is "42501" or "22000" or "P0002" or "22023")
        { return Conflict(new { mensaje = "El servicio no está asignado al técnico o no se encuentra en un estado que permita agendarlo." }); }
    }
}
