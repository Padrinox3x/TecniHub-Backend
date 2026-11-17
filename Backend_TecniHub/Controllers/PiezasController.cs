using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Sprint3;

namespace Backend_TecniHub.Controllers;

[ApiController]
[Route("api/piezas")]
public sealed class PiezasController(ISprint3Service service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PiezaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PiezaDto>>> Buscar([FromQuery] BuscarPiezasRequestDto request, CancellationToken cancellationToken) =>
        Ok(await service.BuscarPiezasAsync(request, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(PiezaPublicadaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PiezaPublicadaDto>> Publicar([FromBody] PublicarPiezaRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.PublicarPiezaAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (PostgresException ex) when (ex.SqlState is "23503" or "23514" or "23502")
        { return BadRequest(new { mensaje = "Los datos no cumplen las referencias o reglas de la base oficial." }); }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        { return Conflict(new { mensaje = "No se pudo registrar la pieza por un folio duplicado. Intenta de nuevo." }); }
    }
}
