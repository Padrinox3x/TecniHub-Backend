
using System.Security.Cryptography;
using System.Text;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Auth;
using TecniHub.Domain.Entities;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Application.Services;

public class RegistroClienteService : IRegistroClienteService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ISesionRepository _sesionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistroClienteService(
        IUsuarioRepository usuarioRepository,
        IClienteRepository clienteRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ISesionRepository sesionRepository,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _clienteRepository = clienteRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _sesionRepository = sesionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> RegistrarAsync(
    RegistroClienteRequestDto request,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = new CorreoElectronico(request.Email)
            .Valor
            .Trim()
            .ToLowerInvariant();

        var existeEnUsuarios = await _usuarioRepository.ExisteEmailAsync(
            email, cancellationToken);

        var existeEnClientes = await _clienteRepository.ExisteEmailAsync(
            email, cancellationToken);
        if (existeEnUsuarios || existeEnClientes)
        {
            throw new InvalidOperationException(
                "No fue posible registrar la cuenta con ese correo.");
        }

        var telefono = new NumeroTelefono(request.Telefono);
        var contrasenaHash = _passwordHasher.Hash(request.Contrasena);

        var usuario = new Usuario(
            email,
            contrasenaHash,
            "CLIENTE");

        var cliente = new Cliente(
            usuario,
            request.Nombre,
            request.Apellidos,
            new CorreoElectronico(email),
            contrasenaHash,
            telefono,
            request.Direccion);

        await _usuarioRepository.AgregarUsuarioAsync(
            usuario, cancellationToken);

        await _clienteRepository.AgregarClienteAsync(
            cliente, cancellationToken);

        // Guarda ambas entidades en la misma operación.
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

        await _sesionRepository.CrearSesionAsync(
            sesion, cancellationToken);

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
