namespace TecniHub.Application.DTOs.Perfil;

public class PerfilResponseDto
{
    public int UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime? FechaUltimoAcceso { get; set; }

    public PerfilClienteDto? Cliente { get; set; }
    public PerfilTecnicoDto? Tecnico { get; set; }
}
