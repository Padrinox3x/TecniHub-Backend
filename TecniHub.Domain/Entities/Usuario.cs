
namespace TecniHub.Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }

    public string Correo { get; private set; } = string.Empty;

    // Aquí se guarda el hash, nunca la contraseña original.
    public string Contrasena { get; private set; } = string.Empty;

    public string Rol { get; private set; } = string.Empty;

    public DateTime? FechaUltimoAcceso { get; private set; }

    // Perfiles asociados a esta cuenta.
    public ICollection<Cliente> Clientes { get; private set; }
        = new List<Cliente>();

    private Usuario() { }

    public Usuario(string correo, string contrasenaHash, string rol)
    {
        if (string.IsNullOrWhiteSpace(correo))
            throw new ArgumentException("El correo es requerido.", nameof(correo));

        if (string.IsNullOrWhiteSpace(contrasenaHash))
            throw new ArgumentException("El hash de contraseña es requerido.", nameof(contrasenaHash));

        if (string.IsNullOrWhiteSpace(rol))
            throw new ArgumentException("El rol es requerido.", nameof(rol));

        Correo = correo.Trim().ToLowerInvariant();
        Contrasena = contrasenaHash;
        Rol = rol.Trim().ToUpperInvariant();
    }

    public void RegistrarAcceso()
    {
        FechaUltimoAcceso = DateTime.UtcNow;
    }

    public void CambiarContrasena(string nuevaContrasenaHash)
    {
        if (string.IsNullOrWhiteSpace(nuevaContrasenaHash))
            throw new ArgumentException(
                "El nuevo hash de contraseña no puede estar vacío.",
                nameof(nuevaContrasenaHash));

        Contrasena = nuevaContrasenaHash;
    }
}
