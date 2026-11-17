namespace TecniHub.Application.DTOs.Servicios;

public sealed record CategoriaDto(
    int Id,
    string Nombre,
    string? Descripcion = null,
    string? IconoUrl = null
);

public sealed record CrearServicioRequestDto(
    string ClienteId,
    int CategoriaId,
    string TipoElectrodomestico,
    string? Marca,
    string? Modelo,
    string DescripcionFalla,
    string UbicacionAproximada,
    string? Municipio,
    DateOnly? FechaSolicitud,
    string? HorarioPreferido,
    string? ImagenReferencia = null
);

public sealed record ServicioCreadoDto(
    string Id,
    string CodigoFolio,
    string Estado
);

public sealed record ServicioDetalleDto(
    int Id,
    string CodigoFolio,
    int ClienteId,
    string ClienteNombre,
    string TipoElectrodomestico,
    string DescripcionFalla,
    string UbicacionAproximada,
    string? Marca,
    string? Modelo,
    string? HorarioPreferido,
    string? ImagenReferencia,
    string Estado,
    int? TecnicoId,
    string? TecnicoNombre,
    DateTime FechaPublicacion,
    DateOnly? FechaProgramada = null,
    TimeOnly? HoraProgramada = null
);
