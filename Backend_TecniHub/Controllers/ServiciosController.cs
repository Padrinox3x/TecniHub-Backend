using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Servicios;

namespace Backend_TecniHub.Controllers;

[ApiController]
[Route("api/servicios")]
public sealed class ServiciosController : ControllerBase
{
    private readonly IServicioService _servicioService;

    public ServiciosController(IServicioService servicioService)
    {
        _servicioService = servicioService;
    }

    /// <summary>
    /// HU-007: Obtiene el catálogo de categorías de electrodomésticos para el formulario de servicio.
    /// </summary>
    [HttpGet("categorias")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> ObtenerCategorias(CancellationToken cancellationToken)
    {
        var categorias = await _servicioService.ObtenerCategoriasAsync(cancellationToken);
        return Ok(categorias);
    }

    /// <summary>
    /// HU-007: Permite a un cliente publicar su electrodoméstico averiado para que los técnicos puedan visualizarlo en el tablón.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServicioCreadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServicioCreadoDto>> Crear(
        [FromBody] CrearServicioRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var result = await _servicioService.CrearServicioAsync(request, usuarioId, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
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
    /// HU-007: Consulta la lista de servicios publicados con filtros opcionales de estado o cliente.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ServicioDetalleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServicioDetalleDto>>> Listar(
        [FromQuery] string? estado,
        [FromQuery] int? clienteId,
        CancellationToken cancellationToken)
    {
        var servicios = await _servicioService.ObtenerServiciosAsync(estado, clienteId, cancellationToken);
        return Ok(servicios);
    }

    /// <summary>
    /// HU-007: Obtiene el detalle de un servicio publicado por su identificador.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServicioDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServicioDetalleDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var servicio = await _servicioService.ObtenerPorIdAsync(id, cancellationToken);
        if (servicio is null)
        {
            return NotFound(new { mensaje = $"No se encontró el servicio con ID {id}." });
        }
        return Ok(servicio);
    }

    /// <summary>
    /// HU-010: Permite marcar un servicio en proceso como finalizado/completado.
    /// </summary>
    [HttpPut("{id:int}/finalizar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Finalizar(int id, CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            await _servicioService.FinalizarServicioAsync(id, usuarioId, cancellationToken);
            return Ok(new { mensaje = "Servicio marcado como finalizado exitosamente." });
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

    /// <summary>
    /// HU-014: El técnico solicita el cierre del servicio una vez finalizado el trabajo físico.
    /// Dispara notificación al cliente para pedir su confirmación y calificación.
    /// </summary>
    [HttpPost("{id:int}/solicitar-cierre")]
    [Authorize]
    [ProducesResponseType(typeof(CierreServicioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CierreServicioResponseDto>> SolicitarCierre(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var resultado = await _servicioService.SolicitarCierreAsync(id, usuarioId, cancellationToken);
            return Ok(resultado);
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

    /// <summary>
    /// HU-014: El cliente confirma la finalización del servicio y emite su evaluación obligatoria (1 a 5 estrellas).
    /// El servicio pasa a estado 'Completado' (borde Azul) y la reseña se suma al perfil del técnico.
    /// </summary>
    [HttpPost("{id:int}/confirmar-cierre")]
    [Authorize]
    [ProducesResponseType(typeof(CierreServicioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CierreServicioResponseDto>> ConfirmarCierre(
        int id,
        [FromBody] ConfirmarCierreYCalificarRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var reqNormalizado = request with { ServicioId = id };
            var resultado = await _servicioService.ConfirmarCierreYCalificarAsync(reqNormalizado, usuarioId, cancellationToken);
            return Ok(resultado);
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

    /// <summary>
    /// HU-014: Cambia el estado de una publicación de servicio (ej. /api/servicios/{id}/estado o /api/servicio/estado).
    /// </summary>
    [HttpPut("{id:int}/estado")]
    [HttpPut("/api/servicio/estado")]
    [Authorize]
    [ProducesResponseType(typeof(ServicioDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServicioDetalleDto>> CambiarEstado(
        [FromRoute] int? id,
        [FromBody] CambiarEstadoServicioRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var servicioId = id ?? request.ServicioId
                ?? throw new ArgumentException("El identificador del servicio es requerido.");

            var usuarioId = ObtenerUsuarioIdActual();
            var resultado = await _servicioService.CambiarEstadoAsync(servicioId, request.NuevoEstado, usuarioId, cancellationToken);
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
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
