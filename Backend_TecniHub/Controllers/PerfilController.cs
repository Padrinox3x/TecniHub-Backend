using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Perfil;
using TecniHub.Domain.Exceptions;

namespace Backend_TecniHub.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PerfilController : ControllerBase
{
    private readonly IPerfilService _perfilService;
    private readonly IPerfilPublicoService _perfilPublicoService;

    public PerfilController(
        IPerfilService perfilService,
        IPerfilPublicoService perfilPublicoService)
    {
        _perfilService = perfilService;
        _perfilPublicoService = perfilPublicoService;
    }

    [HttpGet]
    public async Task<ActionResult<PerfilResponseDto>> ObtenerPerfil(
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Token no válido o no contiene identificación de usuario." });
        }

        try
        {
            var perfil = await _perfilService.ObtenerPerfilAsync(
                usuarioId.Value,
                cancellationToken);

            return Ok(perfil);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPut]
    public async Task<ActionResult<PerfilResponseDto>> EditarPerfil(
        [FromBody] EditarPerfilRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Token no válido o no contiene identificación de usuario." });
        }

        try
        {
            var perfil = await _perfilService.EditarPerfilAsync(
                usuarioId.Value,
                request,
                cancellationToken);

            return Ok(perfil);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("cliente")]
    public async Task<ActionResult<PerfilResponseDto>> EditarPerfilCliente(
        [FromBody] EditarPerfilClienteRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Token no válido o no contiene identificación de usuario." });
        }

        try
        {
            var perfil = await _perfilService.EditarPerfilClienteAsync(
                usuarioId.Value,
                request,
                cancellationToken);

            return Ok(perfil);
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

    [HttpPut("tecnico")]
    public async Task<ActionResult<PerfilResponseDto>> EditarPerfilTecnico(
        [FromBody] EditarPerfilTecnicoRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId is null)
        {
            return Unauthorized(new { mensaje = "Token no válido o no contiene identificación de usuario." });
        }

        try
        {
            var perfil = await _perfilService.EditarPerfilTecnicoAsync(
                usuarioId.Value,
                request,
                cancellationToken);

            return Ok(perfil);
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
    /// HU-012: Visualizar el perfil público de un técnico, incluyendo calificación de hasta 5 estrellas, reputación y reseñas de trabajos anteriores.
    /// </summary>
    [HttpGet("tecnico/publico/{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PerfilPublicoTecnicoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PerfilPublicoTecnicoDto>> ObtenerPerfilPublicoTecnico(
        int id,
        CancellationToken cancellationToken)
    {
        var perfilPublico = await _perfilPublicoService.ObtenerPerfilPublicoTecnicoAsync(id, cancellationToken);
        return Ok(perfilPublico);
    }

    /// <summary>
    /// HU-012: Emisión de una reseña con calificación (1 a 5 estrellas) para un servicio completado.
    /// </summary>
    [HttpPost("tecnico/{id:int}/resenas")]
    [Authorize]
    [ProducesResponseType(typeof(ResenaTecnicoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResenaTecnicoDto>> AgregarResena(
        int id,
        [FromBody] CrearResenaRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdActual();
            var resena = await _perfilPublicoService.AgregarResenaAsync(id, request, usuarioId, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, resena);
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
    /// HU-013: Visualizar el perfil público básico de un cliente (nombre de pila, zona general y estadística de servicios).
    /// Requiere sesión activa para proteger la identidad del cliente (visitantes externos reciben 401).
    /// </summary>
    [HttpGet("cliente/publico/{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(PerfilPublicoClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PerfilPublicoClienteDto>> ObtenerPerfilPublicoCliente(
        int id,
        CancellationToken cancellationToken)
    {
        var perfilPublico = await _perfilPublicoService.ObtenerPerfilPublicoClienteAsync(id, cancellationToken);
        return Ok(perfilPublico);
    }

    private int? ObtenerUsuarioIdActual()
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value;

        return int.TryParse(claimValue, out var id) ? id : null;
    }
}
