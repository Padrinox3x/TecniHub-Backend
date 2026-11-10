namespace TecniHub.Domain.Entities;

/// <summary>Proyección de core.piezas para publicar y buscar ofertas de piezas.</summary>
public sealed class PiezaSprint3
{
    public Guid Id { get; set; }
    public string CodigoFolio { get; set; } = string.Empty;
    public Guid TecnicoId { get; set; }
    public short CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Condicion { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public decimal? Precio { get; set; }
    public string? PiezaIntercambioDeseada { get; set; }
    public string MedioContacto { get; set; } = string.Empty;
    public string UbicacionEntrega { get; set; } = string.Empty;
    public string Estado { get; set; } = "DISPONIBLE";
    public Guid? CompradorTecnicoId { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}

/// <summary>Modelo de lectura para core.categorias_electrodomesticos.</summary>
public sealed class CategoriaSprint3
{
    public short Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? IconoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime? DeletedAt { get; set; }
}

/// <summary>Modelo para core.itinerario_tecnico con los tipos de la BD oficial.</summary>
public sealed class ItinerarioSprint3
{
    public Guid Id { get; set; }
    public Guid TecnicoId { get; set; }
    public Guid ServicioId { get; set; }
    public DateOnly FechaProgramada { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFinEstimada { get; set; }
    public string? NotasCita { get; set; }
    public string EstadoCita { get; set; } = "PROGRAMADO";
    public bool IsActive { get; set; }
    public DateTime? DeletedAt { get; set; }
}
