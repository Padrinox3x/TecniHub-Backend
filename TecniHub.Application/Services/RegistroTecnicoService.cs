using System.Security.Cryptography;
using System.Text;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Auth;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Application.Services;

public class RegistroTecnicoService : IRegistroTecnicoService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITecnicoRepository _tecnicoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ISesionRepository _sesionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistroTecnicoService(
        IUsuarioRepository usuarioRepository,
        ITecnicoRepository tecnicoRepository,
        IClienteRepository clienteRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ISesionRepository sesionRepository,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _tecnicoRepository = tecnicoRepository;
        _clienteRepository = clienteRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _sesionRepository = sesionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> RegistrarAsync(
        RegistroTecnicoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var emailVo = new CorreoElectronico(request.Email);
        var email = emailVo.Valor.Trim().ToLowerInvariant();

        if (await _usuarioRepository.ExisteEmailAsync(email, cancellationToken)
            || await _tecnicoRepository.ExisteEmailAsync(email, cancellationToken)
            || await _clienteRepository.ExisteEmailAsync(email, cancellationToken))
        {
            throw new InvalidOperationException("No fue posible registrar la cuenta con ese correo.");
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

        var contrasenaHash = _passwordHasher.Hash(request.Contrasena);

        var usuario = new Usuario(
            email,
            contrasenaHash,
            "TECNICO");

        var tecnico = new Tecnico(
            usuario,
            request.NombreCompleto,
            emailVo,
            contrasenaHash,
            telefonoVo,
            request.FotoIdentificacion,
            request.TieneNegocioFisico,
            telefonoNegocioVo,
            request.UbicacionEstablecimiento,
            request.RedesSociales,
            request.CostoCotizacion,
            request.EvidenciaTrabajos);

        await _usuarioRepository.AgregarUsuarioAsync(usuario, cancellationToken);
        await _tecnicoRepository.AgregarTecnicoAsync(tecnico, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(
            usuario.Id,
            usuario.Correo,
            usuario.Rol);

        var expiracion = DateTime.UtcNow.AddHours(8);

        var tokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        var sesion = new Sesion(
            usuario.Id,
            tokenHash,
            null,
            null,
            expiracion);

        await _sesionRepository.CrearSesionAsync(sesion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            Token = token,
            Expiracion = expiracion,
            UsuarioId = usuario.Id,
            Email = usuario.Correo,
            Rol = usuario.Rol
        };
    }
}
