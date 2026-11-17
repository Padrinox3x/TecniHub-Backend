namespace TecniHub.Domain.Entities;

/// <summary>Modelo de persistencia para core.servicios. Sus nombres y tipos siguen la BD oficial.</summary>
public sealed class ServicioSprint3
{
    public Guid Id { get; set; }
    public string CodigoFolio { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public Guid? TecnicoId { get; set; }
    public short CategoriaId { get; set; }
    public string TipoElectrodomestico { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string DescripcionFalla { get; set; } = string.Empty;
    public string UbicacionAproximada { get; set; } = string.Empty;
    public string? Municipio { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public DateOnly FechaSolicitud { get; set; }
    public string? HorarioPreferido { get; set; }
    public string Estado { get; set; } = "ABIERTO";
    public DateTime? FechaAceptacion { get; set; }
    public DateTime? FechaSolicitudCierre { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public string? MotivoCancelacion { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
