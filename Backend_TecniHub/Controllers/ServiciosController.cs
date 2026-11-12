using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Sprint3;

namespace Backend_TecniHub.Controllers;

[ApiController]
[Route("api/servicios")]
public sealed class ServiciosController(ISprint3Service service) : ControllerBase
{
    [HttpGet("categorias")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> Categorias(CancellationToken cancellationToken) =>
        Ok(await service.ObtenerCategoriasAsync(cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(ServicioCreadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServicioCreadoDto>> Crear([FromBody] CrearServicioRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.CrearServicioAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (PostgresException ex) when (ex.SqlState is "23503" or "23514" or "23502")
        { return BadRequest(new { mensaje = "Los datos no cumplen las referencias o reglas de la base oficial." }); }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        { return Conflict(new { mensaje = "La publicación ya existe." }); }
    }
}
