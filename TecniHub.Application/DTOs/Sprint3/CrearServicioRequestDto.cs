using System.ComponentModel.DataAnnotations;

namespace TecniHub.Application.DTOs.Sprint3;

public sealed class CrearServicioRequestDto
{
    [Required] public Guid ClienteId { get; init; }
    [Range(1, short.MaxValue)] public short CategoriaId { get; init; }
    [Required, StringLength(100)] public string TipoElectrodomestico { get; init; } = string.Empty;
    [StringLength(100)] public string? Marca { get; init; }
    [StringLength(100)] public string? Modelo { get; init; }
    [Required, StringLength(3000)] public string DescripcionFalla { get; init; } = string.Empty;
    [Required, StringLength(255)] public string UbicacionAproximada { get; init; } = string.Empty;
    [StringLength(100)] public string? Municipio { get; init; }
    [Range(typeof(decimal), "-90", "90")] public decimal? Latitud { get; init; }
    [Range(typeof(decimal), "-180", "180")] public decimal? Longitud { get; init; }
    public DateOnly? FechaSolicitud { get; init; }
    [StringLength(100)] public string? HorarioPreferido { get; init; }
}

public sealed record ServicioCreadoDto(Guid Id, string CodigoFolio, string Estado);
public sealed record CategoriaDto(short Id, string Nombre, string? Descripcion, string? IconoUrl);
