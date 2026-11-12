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

    public PerfilController(IPerfilService perfilService)
    {
        _perfilService = perfilService;
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

    private int? ObtenerUsuarioIdActual()
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value;

        return int.TryParse(claimValue, out var id) ? id : null;
    }
}
