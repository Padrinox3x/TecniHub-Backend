using System.ComponentModel.DataAnnotations;

namespace TecniHub.Application.DTOs.Tablon;

public class CrearPublicacionServicioRequestDto
{
    [Required(ErrorMessage = "El título de la publicación es obligatorio.")]
    [StringLength(150, ErrorMessage = "El título no puede exceder los 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de electrodoméstico es obligatorio.")]
    [StringLength(100, ErrorMessage = "El tipo de electrodoméstico no puede exceder los 100 caracteres.")]
    public string TipoElectrodomestico { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción de la falla es obligatoria.")]
    [StringLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres.")]
    public string DescripcionFalla { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ubicación aproximada es obligatoria.")]
    [StringLength(255, ErrorMessage = "La ubicación no puede exceder los 255 caracteres.")]
    public string UbicacionAproximada { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ImagenReferencia { get; set; }
}
