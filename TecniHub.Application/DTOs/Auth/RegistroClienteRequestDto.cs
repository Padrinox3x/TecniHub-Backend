
using System.ComponentModel.DataAnnotations;

namespace TecniHub.Application.DTOs.Auth;

public class RegistroClienteRequestDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[0-9]{10}$",
        ErrorMessage = "El teléfono debe contener 10 dígitos.")]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Direccion { get; set; }
}
