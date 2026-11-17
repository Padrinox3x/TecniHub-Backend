using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Perfil;

namespace Backend_TecniHub.Controllers;

[ApiController]
[Route("api/cliente")]
public sealed class ClientePublicoController : ControllerBase
{
    private readonly IPerfilPublicoService _perfilPublicoService;

    public ClientePublicoController(IPerfilPublicoService perfilPublicoService)
    {
        _perfilPublicoService = perfilPublicoService;
    }

    /// <summary>
    /// HU-013: GET /api/cliente/publico/{id} - Visualizar el perfil público básico del cliente.
    /// Excluye estrictamente datos sensibles (teléfono, email, dirección exacta).
    /// </summary>
    [HttpGet("publico/{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(PerfilPublicoClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PerfilPublicoClienteDto>> ObtenerPerfilPublico(
        int id,
        CancellationToken cancellationToken)
    {
        var perfil = await _perfilPublicoService.ObtenerPerfilPublicoClienteAsync(id, cancellationToken);
        return Ok(perfil);
    }
}

[ApiController]
[Route("api/tecnico")]
public sealed class TecnicoPublicoController : ControllerBase
{
    private readonly IPerfilPublicoService _perfilPublicoService;

    public TecnicoPublicoController(IPerfilPublicoService perfilPublicoService)
    {
        _perfilPublicoService = perfilPublicoService;
    }

    /// <summary>
    /// HU-012: GET /api/tecnico/publico/{id} - Visualizar el perfil público y reputación del técnico.
    /// </summary>
    [HttpGet("publico/{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PerfilPublicoTecnicoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PerfilPublicoTecnicoDto>> ObtenerPerfilPublico(
        int id,
        CancellationToken cancellationToken)
    {
        var perfil = await _perfilPublicoService.ObtenerPerfilPublicoTecnicoAsync(id, cancellationToken);
        return Ok(perfil);
    }
}
