namespace TecniHub.Application.DTOs.Perfil;

public sealed record PerfilPublicoTecnicoDto(
    int Id,
    string NombreCompleto,
    string? FotoUrl,
    decimal? CalificacionPromedio,
    int TotalTrabajosCompletados,
    bool EsTecnicoNuevo,
    string MensajeReputacion,
    IReadOnlyList<string> Especialidades,
    decimal? CostoDiagnostico,
    bool TieneNegocioFisico,
    string? UbicacionNegocio,
    string MiembroDesde,
    IReadOnlyList<string> EvidenciaTrabajos,
    IReadOnlyList<ResenaTecnicoDto> Resenas
);

public sealed record ResenaTecnicoDto(
    string Id,
    string ClienteNombre,
    short CalificacionEstrellas,
    string FechaRelativa,
    DateTime Fecha,
    string Comentario,
    string TipoServicio
);

public sealed record CrearResenaRequestDto(
    int ServicioId,
    short Calificacion,
    string? Comentario = null
);

public sealed record PerfilPublicoClienteDto(
    int Id,
    string NombrePublico,
    string ZonaAproximada,
    string MiembroDesde,
    int ServiciosSolicitados,
    int ServiciosCompletados,
    string PorcentajeInteraccionesExitosas,
    bool EsUsuarioFrecuente
);
