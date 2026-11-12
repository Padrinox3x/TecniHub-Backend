using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Perfil;
using TecniHub.Domain.Exceptions;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Application.Services;

public class PerfilService : IPerfilService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ITecnicoRepository _tecnicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PerfilService(
        IUsuarioRepository usuarioRepository,
        IClienteRepository clienteRepository,
        ITecnicoRepository tecnicoRepository,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _clienteRepository = clienteRepository;
        _tecnicoRepository = tecnicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PerfilResponseDto> ObtenerPerfilAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        var response = new PerfilResponseDto
        {
            UsuarioId = usuario.Id,
            Email = usuario.Correo,
            Rol = usuario.Rol,
            FechaUltimoAcceso = usuario.FechaUltimoAcceso
        };

        if (string.Equals(usuario.Rol, "CLIENTE", StringComparison.OrdinalIgnoreCase))
        {
            var cliente = await _clienteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
            if (cliente is not null)
            {
                response.Cliente = new PerfilClienteDto
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Apellidos = cliente.Apellidos,
                    Email = cliente.Correo.Valor,
                    Telefono = cliente.NumeroTelefono.Valor,
                    Direccion = cliente.Direccion
                };
            }
        }
        else if (string.Equals(usuario.Rol, "TECNICO", StringComparison.OrdinalIgnoreCase))
        {
            var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
            if (tecnico is not null)
            {
                response.Tecnico = new PerfilTecnicoDto
                {
                    Id = tecnico.Id,
                    NombreCompleto = tecnico.NombreCompleto,
                    Email = tecnico.Correo.Valor,
                    Telefono = tecnico.NumeroTelefono.Valor,
                    TieneNegocioFisico = tecnico.TieneNegocioFisico,
                    TelefonoNegocio = tecnico.TelefonoNegocio?.Valor,
                    UbicacionEstablecimiento = tecnico.UbicacionEstablecimiento,
                    FotoIdentificacion = tecnico.FotoIdentificacion,
                    RedesSociales = tecnico.RedesSociales,
                    CostoCotizacion = tecnico.CostoCotizacion,
                    EvidenciaTrabajos = tecnico.EvidenciaTrabajos
                };
            }
        }

        return response;
    }

    public async Task<PerfilResponseDto> EditarPerfilClienteAsync(
        int usuarioId,
        EditarPerfilClienteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        var cliente = await _clienteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
        if (cliente is null)
        {
            throw new KeyNotFoundException("Perfil de cliente no encontrado para este usuario.");
        }

        var telefonoVo = new NumeroTelefono(request.Telefono);

        cliente.ActualizarPerfil(
            request.Nombre,
            request.Apellidos,
            telefonoVo,
            request.Direccion);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerPerfilAsync(usuarioId, cancellationToken);
    }

    public async Task<PerfilResponseDto> EditarPerfilTecnicoAsync(
        int usuarioId,
        EditarPerfilTecnicoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
        if (tecnico is null)
        {
            throw new KeyNotFoundException("Perfil de técnico no encontrado para este usuario.");
        }

        var telefonoVo = new NumeroTelefono(request.Telefono);
        NumeroTelefono? telefonoNegocioVo = null;

        if (request.TieneNegocioFisico)
        {
            if (string.IsNullOrWhiteSpace(request.UbicacionEstablecimiento))
            {
                throw new ReglaNegocioInvalidaException(
                    "La ubicación del establecimiento es requerida cuando el técnico tiene negocio físico.");
            }

            if (!string.IsNullOrWhiteSpace(request.TelefonoNegocio))
            {
                telefonoNegocioVo = new NumeroTelefono(request.TelefonoNegocio);
            }
        }

        tecnico.ActualizarPerfil(
            request.NombreCompleto,
            telefonoVo,
            request.TieneNegocioFisico,
            telefonoNegocioVo,
            request.UbicacionEstablecimiento,
            request.FotoIdentificacion,
            request.RedesSociales,
            request.CostoCotizacion,
            request.EvidenciaTrabajos);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerPerfilAsync(usuarioId, cancellationToken);
    }

    public async Task<PerfilResponseDto> EditarPerfilAsync(
        int usuarioId,
        EditarPerfilRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        if (string.Equals(usuario.Rol, "CLIENTE", StringComparison.OrdinalIgnoreCase))
        {
            var cliente = await _clienteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
            if (cliente is null)
            {
                throw new KeyNotFoundException("Perfil de cliente no encontrado para este usuario.");
            }

            var requestCliente = new EditarPerfilClienteRequestDto
            {
                Nombre = !string.IsNullOrWhiteSpace(request.Nombre) ? request.Nombre : cliente.Nombre,
                Apellidos = !string.IsNullOrWhiteSpace(request.Apellidos) ? request.Apellidos : cliente.Apellidos,
                Telefono = !string.IsNullOrWhiteSpace(request.Telefono) ? request.Telefono : cliente.NumeroTelefono.Valor,
                Direccion = request.Direccion ?? cliente.Direccion
            };

            return await EditarPerfilClienteAsync(usuarioId, requestCliente, cancellationToken);
        }
        else if (string.Equals(usuario.Rol, "TECNICO", StringComparison.OrdinalIgnoreCase))
        {
            var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
            if (tecnico is null)
            {
                throw new KeyNotFoundException("Perfil de técnico no encontrado para este usuario.");
            }

            var requestTecnico = new EditarPerfilTecnicoRequestDto
            {
                NombreCompleto = !string.IsNullOrWhiteSpace(request.NombreCompleto) ? request.NombreCompleto : tecnico.NombreCompleto,
                Telefono = !string.IsNullOrWhiteSpace(request.Telefono) ? request.Telefono : tecnico.NumeroTelefono.Valor,
                TieneNegocioFisico = request.TieneNegocioFisico ?? tecnico.TieneNegocioFisico,
                TelefonoNegocio = request.TelefonoNegocio ?? tecnico.TelefonoNegocio?.Valor,
                UbicacionEstablecimiento = request.UbicacionEstablecimiento ?? tecnico.UbicacionEstablecimiento,
                FotoIdentificacion = request.FotoIdentificacion ?? tecnico.FotoIdentificacion,
                RedesSociales = request.RedesSociales ?? tecnico.RedesSociales,
                CostoCotizacion = request.CostoCotizacion ?? tecnico.CostoCotizacion,
                EvidenciaTrabajos = request.EvidenciaTrabajos ?? tecnico.EvidenciaTrabajos
            };

            return await EditarPerfilTecnicoAsync(usuarioId, requestTecnico, cancellationToken);
        }

        throw new InvalidOperationException($"Rol de usuario '{usuario.Rol}' no soportado para edición de perfil.");
    }
}
