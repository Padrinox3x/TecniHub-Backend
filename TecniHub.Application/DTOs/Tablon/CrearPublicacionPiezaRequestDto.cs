using System.ComponentModel.DataAnnotations;

namespace TecniHub.Application.DTOs.Tablon;

public class CrearPublicacionPiezaRequestDto
{
    [Required(ErrorMessage = "El nombre de la pieza es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre de la pieza no puede exceder los 150 caracteres.")]
    public string NombrePieza { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría del aparato es obligatoria.")]
    [StringLength(100, ErrorMessage = "La categoría no puede exceder los 100 caracteres.")]
    public string CategoriaAparato { get; set; } = string.Empty;

    [Required(ErrorMessage = "La condición de la pieza es obligatoria.")]
    [StringLength(50, ErrorMessage = "La condición no puede exceder los 50 caracteres.")]
    public string CondicionPieza { get; set; } = string.Empty; // ej: "Nueva", "Usada", "Para refacciones"

    [Required(ErrorMessage = "La modalidad de transacción es obligatoria.")]
    public string TipoTransaccion { get; set; } = "Venta"; // "Venta", "Intercambio", "VentaOIntercambio"

    [Range(0, 99999999.99, ErrorMessage = "El precio no puede ser negativo.")]
    public decimal? Precio { get; set; }

    [Required(ErrorMessage = "Los datos de contacto son obligatorios.")]
    [StringLength(255, ErrorMessage = "Los datos de contacto no pueden exceder los 255 caracteres.")]
    public string DatosContacto { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres.")]
    public string? DescripcionDetalle { get; set; }

    [StringLength(500)]
    public string? Fotografia { get; set; }
}
