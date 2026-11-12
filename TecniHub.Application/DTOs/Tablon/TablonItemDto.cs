namespace TecniHub.Application.DTOs.Tablon;

public class TablonItemDto
{
    public int Id { get; set; }
    public string TipoPublicacion { get; set; } = string.Empty; // "SERVICIO" | "PIEZA"
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    public int AutorId { get; set; }
    public string AutorNombre { get; set; } = string.Empty;
    public string AutorRol { get; set; } = string.Empty; // "CLIENTE" | "TECNICO"

    public string Estado { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty; // "Reparacion" | "Venta" | "Intercambio" | "VentaOIntercambio"

    // Sistema de Codificación de Colores (HU-009)
    public string CodigoColor { get; set; } = string.Empty; // "Rojo" | "Verde" | "Azul" | "Amarillo" | "Naranja" | "AmarilloNaranja" | "AzulMarino"
    public string ClaseCssBorde { get; set; } = string.Empty; // ej: "border-red-500", "border-green-500", etc.
    public string ColorHex { get; set; } = string.Empty;
    public bool EsCompletado { get; set; }
    public string? TextoIndicador { get; set; } // "Completado..."

    public decimal? Precio { get; set; }
    public string? Ubicacion { get; set; }
    public string? ImagenUrl { get; set; }
    public DateTime FechaPublicacion { get; set; }

    public int? TecnicoAsignadoId { get; set; }
    public string? TecnicoAsignadoNombre { get; set; }
}
