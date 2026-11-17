namespace TecniHub.Application.DTOs.Tablon;

public class TablonFiltroDto
{
    public string? Tipo { get; set; } // "TODOS" | "SERVICIO" | "PIEZA"
    public string? Estado { get; set; } // "Pendiente", "EnProceso", "Completado", "Disponible"
    public string? Color { get; set; } // "Rojo", "Verde", "Azul", "Amarillo", "Naranja", "AmarilloNaranja", "AzulMarino"
    public bool IncluirCompletados { get; set; } = false; // Por defecto false según la regla de limpieza
}
