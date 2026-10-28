
using Microsoft.AspNetCore.Mvc;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Auth;

namespace Backend_TecniHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IRegistroClienteService _registroClienteService;
    private readonly ILoginService _loginService;

    public AuthController(
        IRegistroClienteService registroClienteService,
        ILoginService loginService)
    {
        _registroClienteService = registroClienteService;
        _loginService = loginService;
    }

    [HttpPost("registro")]
    public async Task<ActionResult<AuthResponseDto>> Registrar(
        [FromBody] RegistroClienteRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var respuesta = await _registroClienteService.RegistrarAsync(
                request,
                cancellationToken);

            return Ok(respuesta);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var respuesta = await _loginService.IniciarSesionAsync(
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString(),
                cancellationToken);

            return Ok(respuesta);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new
            {
                mensaje = "Correo o contraseña incorrectos."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
