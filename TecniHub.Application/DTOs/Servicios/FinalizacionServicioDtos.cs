namespace TecniHub.Application.DTOs.Servicios;

public sealed record SolicitarCierreServicioRequestDto(
    string? ComentarioTecnico = null
);

public sealed record ConfirmarCierreYCalificarRequestDto(
    int ServicioId,
    short Calificacion, // 1 a 5 obligatorio
    string? Comentario = null // Opcional según regla de negocio HU-014
);

public sealed record CambiarEstadoServicioRequestDto(
    int? ServicioId,
    string NuevoEstado, // "Pendiente", "EnProceso", "PendienteCierre", "Completado"
    string? Motivo = null
);

public sealed record CierreServicioResponseDto(
    int ServicioId,
    string Estado,
    string Mensaje,
    string CodigoColor,
    string ColorHex,
    string? TextoIndicador,
    string? ResenaId,
    short? Calificacion,
    string? Comentario,
    DateTime FechaFinalizacion
);

public sealed record CrearResenaUniversalRequestDto(
    int IdServicio,
    int? IdCliente = null,
    int? IdTecnico = null,
    short Calificacion = 5,
    string? Comentario = null
);
