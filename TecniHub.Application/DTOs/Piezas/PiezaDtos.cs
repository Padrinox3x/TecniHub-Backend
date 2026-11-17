namespace TecniHub.Application.DTOs.Piezas;

public sealed record PublicarPiezaRequestDto(
    string TecnicoId,
    int CategoriaId,
    string Nombre,
    string Descripcion,
    string Condicion,
    string Modalidad,
    decimal? Precio,
    string? PiezaIntercambioDeseada,
    string MedioContacto,
    string UbicacionEntrega,
    string? Fotografia = null
);

public sealed record PiezaDto(
    string Id,
    string CodigoFolio,
    int CategoriaId,
    string Nombre,
    string Descripcion,
    string Condicion,
    string Modalidad,
    decimal? Precio,
    string? PiezaIntercambioDeseada,
    string MedioContacto,
    string UbicacionEntrega,
    string? Fotografia = null,
    string EstadoDisponibilidad = "Disponible",
    int TecnicoId = 0,
    DateTime? FechaPublicacion = null
);

public sealed record BuscarPiezasRequestDto(
    string? Texto = null,
    int? CategoriaId = null,
    string? Condicion = null,
    string? Modalidad = null,
    decimal? PrecioMaximo = null
);

public sealed record ActualizarDisponibilidadPiezaDto(
    string EstadoDisponibilidad
);
