using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Perfil;
using TecniHub.Application.DTOs.Servicios;
using TecniHub.Domain.Exceptions;

namespace Backend_TecniHub.Controllers;

[ApiController]
[Route("api/resenas")]
public sealed class ResenasController : ControllerBase
{
    private readonly IPerfilPublicoService _perfilPublicoService;
    private readonly IPublicacionServicioRepository _servicioRepository;

    public ResenasController(
        IPerfilPublicoService perfilPublicoService,
        IPublicacionServicioRepository servicioRepository)
    {
        _perfilPublicoService = perfilPublicoService;
        _servicioRepository = servicioRepository;
    }

    /// <summary>
    /// HU-014: Inserta la calificación y reseña en la base de datos, relacionando ID_Cliente, ID_Tecnico e ID_Servicio.
    /// Valida calificación de 1 a 5 estrellas obligatoria, comentario opcional y unicidad de reseña.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ResenaTecnicoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResenaTecnicoDto>> CrearResena(
        [FromBody] CrearResenaUniversalRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var servicio = await _servicioRepository.ObtenerPorIdAsync(request.IdServicio, cancellationToken);
            if (servicio is null)
            {
                return NotFound(new { mensaje = $"No se encontró el servicio con ID {request.IdServicio}." });
            }

            var tecnicoId = request.IdTecnico ?? servicio.TecnicoId
                ?? throw new ReglaNegocioInvalidaException("El servicio no cuenta con un técnico asignado.");

            var usuarioId = ObtenerUsuarioIdActual();

            var reqDto = new CrearResenaRequestDto(
                ServicioId: request.IdServicio,
                Calificacion: request.Calificacion,
                Comentario: request.Comentario
            );

            var resena = await _perfilPublicoService.AgregarResenaAsync(
                tecnicoId,
                reqDto,
                usuarioId,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, resena);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (ReglaNegocioInvalidaException ex)
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
    /// HU-014: Consulta la reseña asociada a un servicio finalizado específico.
    /// </summary>
    [HttpGet("servicio/{servicioId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ResenaTecnicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResenaTecnicoDto>> ObtenerPorServicio(
        int servicioId,
        CancellationToken cancellationToken)
    {
        var resena = await _perfilPublicoService.ObtenerResenaPorServicioIdAsync(servicioId, cancellationToken);
        if (resena is null)
        {
            return NotFound(new { mensaje = $"No existe una reseña registrada para el servicio con ID {servicioId}." });
        }

        return Ok(resena);
    }

    /// <summary>
    /// HU-012/HU-014: Lista todas las reseñas registradas para un técnico.
    /// </summary>
    [HttpGet("tecnico/{tecnicoId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ResenaTecnicoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResenaTecnicoDto>>> ObtenerPorTecnico(
        int tecnicoId,
        CancellationToken cancellationToken)
    {
        var resenas = await _perfilPublicoService.ObtenerResenasPorTecnicoIdAsync(tecnicoId, cancellationToken);
        return Ok(resenas);
    }

    private int? ObtenerUsuarioIdActual()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
