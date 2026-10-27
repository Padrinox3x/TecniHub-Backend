using TecniHub.Domain.Exceptions;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Domain.Entities;

public class Tecnico
{
    public int Id { get; private set; }
    public string NombreCompleto { get; private set; }

    public CorreoElectronico Correo { get; private set; }

    public string Contrasena { get; private set; }

    public NumeroTelefono NumeroTelefono { get; private set; }

    public string? FotoIdentificacion { get; private set; }
    public bool TieneNegocioFisico { get; private set; }

    public NumeroTelefono? TelefonoNegocio { get; private set; }

    public string? UbicacionEstablecimiento { get; private set; }
    public string? RedesSociales { get; private set; }
    public decimal? CostoCotizacion { get; private set; }
    public string? EvidenciaTrabajos { get; private set; }
    public string Rol { get; private set; }

    private Tecnico() { }

    public Tecnico(string nombreCompleto, CorreoElectronico correo, string contrasenaHash, NumeroTelefono numeroTelefono)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
            throw new ReglaNegocioInvalidaException("El nombre completo es requerido.");

        NombreCompleto = nombreCompleto;
        Correo = correo;
        Contrasena = contrasenaHash;
        NumeroTelefono = numeroTelefono;
        TieneNegocioFisico = false;
        Rol = "Técnico";
    }

    public void ConfigurarNegocioFisico(NumeroTelefono telefonoNegocio, string ubicacionEstablecimiento)
    {
        if (string.IsNullOrWhiteSpace(ubicacionEstablecimiento))
            throw new ReglaNegocioInvalidaException("La ubicación es requerida para registrar un negocio físico.");

        TieneNegocioFisico = true;
        TelefonoNegocio = telefonoNegocio;
        UbicacionEstablecimiento = ubicacionEstablecimiento;
    }

    public void ActualizarCostoCotizacion(decimal nuevoCosto)
    {
        if (nuevoCosto < 0)
            throw new PrecioInvalidoException(nuevoCosto);

        CostoCotizacion = nuevoCosto;  
    }

    public void CambiarContrasena(string nuevaContrasenaHash)
    {
        if (string.IsNullOrWhiteSpace(nuevaContrasenaHash))
            throw new ReglaNegocioInvalidaException("La nueva contraseña no puede estar vacía.");

        Contrasena = nuevaContrasenaHash;
    }
}