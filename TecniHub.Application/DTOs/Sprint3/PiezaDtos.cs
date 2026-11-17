using System.ComponentModel.DataAnnotations;

namespace TecniHub.Application.DTOs.Sprint3;

public sealed class PublicarPiezaRequestDto
{
    [Required] public Guid TecnicoId { get; init; }
    [Range(1, short.MaxValue)] public short CategoriaId { get; init; }
    [Required, StringLength(150)] public string Nombre { get; init; } = string.Empty;
    [Required, StringLength(3000)] public string Descripcion { get; init; } = string.Empty;
    [Required] public string Condicion { get; init; } = string.Empty;
    [Required] public string Modalidad { get; init; } = string.Empty;
    [Range(typeof(decimal), "0.01", "99999999.99")] public decimal? Precio { get; init; }
    [StringLength(1000)] public string? PiezaIntercambioDeseada { get; init; }
    [Required, StringLength(255)] public string MedioContacto { get; init; } = string.Empty;
    [Required, StringLength(255)] public string UbicacionEntrega { get; init; } = string.Empty;
}

public sealed class BuscarPiezasRequestDto
{
    [StringLength(150)] public string? Texto { get; init; }
    public short? CategoriaId { get; init; }
    public string? Condicion { get; init; }
    public string? Modalidad { get; init; }
    [Range(typeof(decimal), "0", "99999999.99")] public decimal? PrecioMaximo { get; init; }
}

public sealed record PiezaDto(Guid Id, string CodigoFolio, short CategoriaId, string Nombre,
    string Descripcion, string Condicion, string Modalidad, decimal? Precio,
    string? PiezaIntercambioDeseada, string MedioContacto, string UbicacionEntrega);

public sealed record PiezaPublicadaDto(Guid Id);
