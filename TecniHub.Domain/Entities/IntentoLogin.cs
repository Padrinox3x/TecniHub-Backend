
namespace TecniHub.Domain.Entities;

public class IntentoLogin
{
    public long Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public int? UsuarioId { get; private set; }

    public bool Exitoso { get; private set; }

    public string IpOrigen { get; private set; } = string.Empty;

    public string? UserAgent { get; private set; }

    public string? MotivoFallo { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Usuario? Usuario { get; private set; }

    private IntentoLogin() { }

    public IntentoLogin(
        string email,
        int? usuarioId,
        bool exitoso,
        string? ipOrigen,
        string? userAgent,
        string? motivoFallo = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El correo es requerido.");

        if (exitoso && usuarioId is null)
            throw new ArgumentException(
                "Un inicio de sesión exitoso debe estar asociado a un usuario.");

        if (exitoso && !string.IsNullOrWhiteSpace(motivoFallo))
            throw new ArgumentException(
                "Un intento exitoso no debe tener motivo de fallo.");

        Email = email.Trim().ToLowerInvariant();
        UsuarioId = usuarioId;
        Exitoso = exitoso;
        IpOrigen = ipOrigen ?? string.Empty;
        UserAgent = userAgent;
        MotivoFallo = motivoFallo;
        CreatedAt = DateTime.UtcNow;
    }
}
