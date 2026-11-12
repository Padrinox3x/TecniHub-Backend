namespace TecniHub.Application.DTOs.Perfil;

public class PerfilTecnicoDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool TieneNegocioFisico { get; set; }
    public string? TelefonoNegocio { get; set; }
    public string? UbicacionEstablecimiento { get; set; }
    public string? FotoIdentificacion { get; set; }
    public string? RedesSociales { get; set; }
    public decimal? CostoCotizacion { get; set; }
    public string? EvidenciaTrabajos { get; set; }
}
