using System.ComponentModel.DataAnnotations;

namespace TecniHub.Application.DTOs.Perfil;

public class EditarPerfilTecnicoRequestDto
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre completo no puede exceder los 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[0-9]{10}$",
        ErrorMessage = "El teléfono debe contener exactamente 10 dígitos numéricos.")]
    public string Telefono { get; set; } = string.Empty;

    public bool TieneNegocioFisico { get; set; }

    [RegularExpression(@"^[0-9]{10}$",
        ErrorMessage = "El teléfono del negocio debe contener exactamente 10 dígitos numéricos.")]
    public string? TelefonoNegocio { get; set; }

    [StringLength(255, ErrorMessage = "La ubicación no puede exceder los 255 caracteres.")]
    public string? UbicacionEstablecimiento { get; set; }

    [StringLength(500)]
    public string? FotoIdentificacion { get; set; }

    [StringLength(500)]
    public string? RedesSociales { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "El costo de cotización no puede ser negativo.")]
    public decimal? CostoCotizacion { get; set; }

    public string? EvidenciaTrabajos { get; set; }
}
