
namespace TecniHub.Domain.Entities;

public class Sesion
{
    public Guid Id { get; private set; }

    public int UsuarioId { get; private set; }

    // Se almacena el hash del token, no el JWT original.
    public string TokenHash { get; private set; } = string.Empty;

    public string IpOrigen { get; private set; } = string.Empty;

    public string? UserAgent { get; private set; }

    public DateTime ExpiraEn { get; private set; }

    public DateTime? RevocadoEn { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    private Sesion() { }

    public Sesion(
        int usuarioId,
        string tokenHash,
        string? ipOrigen,
        string? userAgent,
        DateTime expiraEn)
    {
        if (usuarioId <= 0)
            throw new ArgumentException("El usuario debe ser válido.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("El hash del token es requerido.");

        if (expiraEn <= DateTime.UtcNow)
            throw new ArgumentException("La expiración debe ser futura.");

        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        IpOrigen = ipOrigen ?? string.Empty;
        UserAgent = userAgent;
        ExpiraEn = expiraEn;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void Revocar()
    {
        if (RevocadoEn.HasValue)
            return;

        RevocadoEn = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
