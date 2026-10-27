using TecniHub.Domain.Exceptions;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Domain.Entities;

public class Cliente
{
    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public string Apellidos { get; private set; }
    public NumeroTelefono NumeroTelefono { get; private set; }

    // 1. Aplicación del Value Object en lugar de un string simple
    public CorreoElectronico Correo { get; private set; }

    public string Contrasena { get; private set; }
    public string? Direccion { get; private set; }
    public string Rol { get; private set; }

    private Cliente() { }

    public Cliente(string nombre, string apellidos, CorreoElectronico correo, string contrasenaHash)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaNegocioInvalidaException("El nombre del cliente es requerido.");

        Nombre = nombre;
        Apellidos = apellidos;
        Correo = correo;
        Contrasena = contrasenaHash;
        Rol = "Cliente";
    }

    public void ActualizarDatosDeContacto(string? numeroTelefono, string? direccion)
    {
        NumeroTelefono = NumeroTelefono;
        Direccion = direccion;
    }

    public void CambiarContrasena(string nuevaContrasenaHash)
    {
        if (string.IsNullOrWhiteSpace(nuevaContrasenaHash))
            throw new ReglaNegocioInvalidaException("La nueva contraseña no puede estar vacía.");

        Contrasena = nuevaContrasenaHash;
    }
}