using System.Security.Cryptography;
using System.Text;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Auth;
using TecniHub.Domain.Entities;

namespace TecniHub.Application.Services;

public class LoginService : ILoginService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ISesionRepository _sesionRepository;
    private readonly IIntentoLoginRepository _intentoLoginRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginService(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ISesionRepository sesionRepository,
        IIntentoLoginRepository intentoLoginRepository,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _sesionRepository = sesionRepository;
        _intentoLoginRepository = intentoLoginRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> IniciarSesionAsync(
        LoginRequestDto request,
        string? ipOrigen,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = request.Email.Trim().ToLowerInvariant();

        var usuario = await _usuarioRepository.ObtenerPorEmailAsync(
            email, cancellationToken);

        if (usuario is null)
        {
            var intento = new IntentoLogin(
                email,
                null,
                false,
                ipOrigen,
                userAgent,
                "Credenciales inválidas");

            await _intentoLoginRepository.RegistrarIntentoAsync(
                intento, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedAccessException(
                "Correo o contraseña incorrectos.");
        }

        var contrasenaValida = _passwordHasher.Verify(
            request.Contrasena,
            usuario.Contrasena);

        if (!contrasenaValida)
        {
            var intento = new IntentoLogin(
                email,
                usuario.Id,
                false,
                ipOrigen,
                userAgent,
                "Credenciales inválidas");

            await _intentoLoginRepository.RegistrarIntentoAsync(
                intento, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedAccessException(
                "Correo o contraseña incorrectos.");
        }

        usuario.RegistrarAcceso();

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
            ipOrigen,
            userAgent,
            expiracion);

        var intentoExitoso = new IntentoLogin(
            email,
            usuario.Id,
            true,
            ipOrigen,
            userAgent);

        await _sesionRepository.CrearSesionAsync(
            sesion, cancellationToken);

        await _intentoLoginRepository.RegistrarIntentoAsync(
            intentoExitoso, cancellationToken);

        // Persiste también FechaUltimoAcceso.
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
