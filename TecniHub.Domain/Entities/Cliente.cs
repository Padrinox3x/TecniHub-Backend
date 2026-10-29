
using TecniHub.Domain.Exceptions;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Domain.Entities;

public class Cliente
{
    public int Id { get; private set; }

    // Clave foránea hacia la cuenta de usuario.
    public int UsuarioId { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    public string Nombre { get; private set; } = string.Empty;

    public string Apellidos { get; private set; } = string.Empty;

    public NumeroTelefono NumeroTelefono { get; private set; } = null!;

    public CorreoElectronico Correo { get; private set; } = null!;

    // Se conserva para compatibilidad con el esquema actual.
    // En el registro se asignará el mismo hash que a Usuario.
    public string Contrasena { get; private set; } = string.Empty;

    public string? Direccion { get; private set; }

    public string Rol { get; private set; } = "CLIENTE";

    private Cliente() { }


    public Cliente(
        Usuario usuario,
        string nombre,
        string apellidos,
        CorreoElectronico correo,
        string contrasenaHash,
        NumeroTelefono numeroTelefono,
        string? direccion)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaNegocioInvalidaException(
                "El nombre del cliente es requerido.");

        if (string.IsNullOrWhiteSpace(apellidos))
            throw new ReglaNegocioInvalidaException(
                "Los apellidos del cliente son requeridos.");

        if (string.IsNullOrWhiteSpace(contrasenaHash))
            throw new ReglaNegocioInvalidaException(
                "El hash de contraseña es requerido.");

        Usuario = usuario;
        UsuarioId = usuario.Id;
        Nombre = nombre.Trim();
        Apellidos = apellidos.Trim();
        Correo = correo ?? throw new ArgumentNullException(nameof(correo));
        Contrasena = contrasenaHash;
        NumeroTelefono = numeroTelefono
            ?? throw new ArgumentNullException(nameof(numeroTelefono));
        Direccion = string.IsNullOrWhiteSpace(direccion)
            ? null
            : direccion.Trim();
        Rol = "CLIENTE";
    }


    public void ActualizarDatosDeContacto(
        string? numeroTelefono,
        string? direccion)
    {
        if (!string.IsNullOrWhiteSpace(numeroTelefono))
        {
            NumeroTelefono = new NumeroTelefono(numeroTelefono);
        }

        Direccion = string.IsNullOrWhiteSpace(direccion)
            ? null
            : direccion.Trim();
    }

    public void CambiarContrasena(string nuevaContrasenaHash)
    {
        if (string.IsNullOrWhiteSpace(nuevaContrasenaHash))
            throw new ReglaNegocioInvalidaException(
                "La nueva contraseña no puede estar vacía.");

        Contrasena = nuevaContrasenaHash;
    }
}
