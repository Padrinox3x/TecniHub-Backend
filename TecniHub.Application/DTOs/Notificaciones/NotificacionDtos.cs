namespace TecniHub.Application.DTOs.Notificaciones;

public sealed record SuscripcionPushRequestDto(
    string Endpoint,
    string? P256dh = null,
    string? Auth = null,
    string? Plataforma = null,
    string? DispositivoToken = null,
    int? UsuarioId = null
);

public sealed record DesuscripcionPushRequestDto(
    string Endpoint
);

public sealed record NotificacionDto(
    string Id,
    int UsuarioId,
    string Titulo,
    string Mensaje,
    string Tipo, // "ServicioAceptado", "TrabajoCompletado", "NuevoMensaje", "SolicitudCierre", "General"
    string CodigoColor, // "Verde", "Azul", "Amarillo", "Rojo"
    string ColorHex,
    int? ReferenciaId,
    string? TipoReferencia, // "Servicio", "Pieza", "Chat"
    DateTime Fecha,
    string FechaRelativa,
    bool Leida
);

public sealed record EnviarNotificacionRequestDto(
    int UsuarioId,
    string Titulo,
    string Mensaje,
    string Tipo,
    string? CodigoColor = null,
    string? ColorHex = null,
    int? ReferenciaId = null,
    string? TipoReferencia = null
);

public sealed record SimulacionEventoPushRequestDto(
    string TipoEvento, // "TecnicoAceptoServicio", "ClienteFinalizoServicio", "NuevoMensajeChat"
    int? ServicioId = null,
    int? PiezaId = null,
    string? NombreContraparte = null,
    string? TituloItem = null
);

public sealed record EstadoConectividadDto(
    bool Online,
    DateTime Timestamp,
    string Mensaje,
    string Servidor,
    string Version
);
