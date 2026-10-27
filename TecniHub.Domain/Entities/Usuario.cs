namespace TecniHub.Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }
    public string Correo { get; private set; }
    public string Contrasena { get; private set; }
    public string Rol { get; private set; }
    public DateTime? FechaUltimoAcceso { get; private set; } 

    private Usuario() { }

    public Usuario(string correo, string contrasenaHash, string rol)
    {
        if (string.IsNullOrWhiteSpace(correo)) throw new ArgumentException("El correo es requerido.");
        if (string.IsNullOrWhiteSpace(contrasenaHash)) throw new ArgumentException("La contraseña es requerida.");
        if (string.IsNullOrWhiteSpace(rol)) throw new ArgumentException("El rol es requerido.");

        Correo = correo;
        Contrasena = contrasenaHash;
        Rol = rol;
    }

    public void RegistrarAcceso()
    {
     
        FechaUltimoAcceso = DateTime.UtcNow;
    }

    public void CambiarContrasena(string nuevaContrasenaHash)
    {
        if (string.IsNullOrWhiteSpace(nuevaContrasenaHash)) throw new ArgumentException("La nueva contraseña no puede estar vacía.");
        Contrasena = nuevaContrasenaHash;
    }
}