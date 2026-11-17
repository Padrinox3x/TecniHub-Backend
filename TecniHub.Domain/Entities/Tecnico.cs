using TecniHub.Domain.Exceptions;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Domain.Entities;

public class Tecnico
{
    public int Id { get; private set; }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    public string NombreCompleto { get; private set; } = string.Empty;

    public CorreoElectronico Correo { get; private set; } = null!;

    public string Contrasena { get; private set; } = string.Empty;

    public NumeroTelefono NumeroTelefono { get; private set; } = null!;

    public string? FotoIdentificacion { get; private set; }
    public bool TieneNegocioFisico { get; private set; }

    public NumeroTelefono? TelefonoNegocio { get; private set; }

    public string? UbicacionEstablecimiento { get; private set; }
    public string? RedesSociales { get; private set; }
    public decimal? CostoCotizacion { get; private set; }
    public string? EvidenciaTrabajos { get; private set; }
    public string Rol { get; private set; } = "TECNICO";

    private Tecnico() { }

    public Tecnico(
        Usuario usuario,
        string nombreCompleto,
        CorreoElectronico correo,
        string contrasenaHash,
        NumeroTelefono numeroTelefono,
        string? fotoIdentificacion = null,
        bool tieneNegocioFisico = false,
        NumeroTelefono? telefonoNegocio = null,
        string? ubicacionEstablecimiento = null,
        string? redesSociales = null,
        decimal? costoCotizacion = null,
        string? evidenciaTrabajos = null)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        if (string.IsNullOrWhiteSpace(nombreCompleto))
            throw new ReglaNegocioInvalidaException("El nombre completo es requerido.");

        if (string.IsNullOrWhiteSpace(contrasenaHash))
            throw new ReglaNegocioInvalidaException("El hash de contraseña es requerido.");

        if (tieneNegocioFisico)
        {
            if (string.IsNullOrWhiteSpace(ubicacionEstablecimiento))
                throw new ReglaNegocioInvalidaException("La ubicación es requerida para registrar un negocio físico.");

            if (telefonoNegocio is null)
                throw new ReglaNegocioInvalidaException("El teléfono del negocio es requerido para registrar un negocio físico.");
        }

        if (costoCotizacion.HasValue && costoCotizacion.Value < 0)
            throw new PrecioInvalidoException(costoCotizacion.Value);

        Usuario = usuario;
        UsuarioId = usuario.Id;
        NombreCompleto = nombreCompleto.Trim();
        Correo = correo ?? throw new ArgumentNullException(nameof(correo));
        Contrasena = contrasenaHash;
        NumeroTelefono = numeroTelefono ?? throw new ArgumentNullException(nameof(numeroTelefono));
        FotoIdentificacion = string.IsNullOrWhiteSpace(fotoIdentificacion) ? null : fotoIdentificacion.Trim();
        TieneNegocioFisico = tieneNegocioFisico;
        TelefonoNegocio = tieneNegocioFisico ? telefonoNegocio : null;
        UbicacionEstablecimiento = tieneNegocioFisico ? ubicacionEstablecimiento?.Trim() : null;
        RedesSociales = string.IsNullOrWhiteSpace(redesSociales) ? null : redesSociales.Trim();
        CostoCotizacion = costoCotizacion;
        EvidenciaTrabajos = string.IsNullOrWhiteSpace(evidenciaTrabajos) ? null : evidenciaTrabajos.Trim();
        Rol = "TECNICO";
    }

    public Tecnico(string nombreCompleto, CorreoElectronico correo, string contrasenaHash, NumeroTelefono numeroTelefono)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
            throw new ReglaNegocioInvalidaException("El nombre completo es requerido.");

        NombreCompleto = nombreCompleto.Trim();
        Correo = correo ?? throw new ArgumentNullException(nameof(correo));
        Contrasena = contrasenaHash;
        NumeroTelefono = numeroTelefono ?? throw new ArgumentNullException(nameof(numeroTelefono));
        TieneNegocioFisico = false;
        Rol = "TECNICO";
    }

    public void ActualizarPerfil(
        string nombreCompleto,
        NumeroTelefono numeroTelefono,
        bool tieneNegocioFisico,
        NumeroTelefono? telefonoNegocio,
        string? ubicacionEstablecimiento,
        string? fotoIdentificacion,
        string? redesSociales,
        decimal? costoCotizacion,
        string? evidenciaTrabajos)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
            throw new ReglaNegocioInvalidaException("El nombre completo es requerido.");

        if (tieneNegocioFisico)
        {
            if (string.IsNullOrWhiteSpace(ubicacionEstablecimiento))
                throw new ReglaNegocioInvalidaException("La ubicación es requerida para registrar un negocio físico.");

            if (telefonoNegocio is null)
                throw new ReglaNegocioInvalidaException("El teléfono del negocio es requerido para registrar un negocio físico.");
        }

        if (costoCotizacion.HasValue && costoCotizacion.Value < 0)
            throw new PrecioInvalidoException(costoCotizacion.Value);

        NombreCompleto = nombreCompleto.Trim();
        NumeroTelefono = numeroTelefono ?? throw new ArgumentNullException(nameof(numeroTelefono));
        TieneNegocioFisico = tieneNegocioFisico;
        TelefonoNegocio = tieneNegocioFisico ? telefonoNegocio : null;
        UbicacionEstablecimiento = tieneNegocioFisico ? ubicacionEstablecimiento?.Trim() : null;
        FotoIdentificacion = string.IsNullOrWhiteSpace(fotoIdentificacion) ? null : fotoIdentificacion.Trim();
        RedesSociales = string.IsNullOrWhiteSpace(redesSociales) ? null : redesSociales.Trim();
        CostoCotizacion = costoCotizacion;
        EvidenciaTrabajos = string.IsNullOrWhiteSpace(evidenciaTrabajos) ? null : evidenciaTrabajos.Trim();
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