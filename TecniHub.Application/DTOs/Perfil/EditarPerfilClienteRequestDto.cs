using System.ComponentModel.DataAnnotations;

namespace TecniHub.Application.DTOs.Perfil;

public class EditarPerfilClienteRequestDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder los 100 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[0-9]{10}$",
        ErrorMessage = "El teléfono debe contener exactamente 10 dígitos numéricos.")]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La dirección no puede exceder los 255 caracteres.")]
    public string? Direccion { get; set; }
}
