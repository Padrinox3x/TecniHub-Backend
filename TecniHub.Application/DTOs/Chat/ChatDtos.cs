namespace TecniHub.Application.DTOs.Chat;

public sealed record EnviarMensajeRequestDto(
    string Contenido,
    int? RemitenteUsuarioId = null
);

public sealed record MensajeDto(
    long Id,
    string ConversacionId,
    int EmisorId,
    string EmisorNombre,
    string EmisorRol,
    string Contenido,
    DateTime EnviadoAt,
    string HoraFormateada,
    bool EsMio = false
);

public sealed record ConversacionResumenDto(
    string Id,
    string TipoContexto,
    int ContextoId,
    string TituloContexto,
    string EstadoContexto,
    int ClienteUsuarioId,
    string ClienteNombre,
    int TecnicoUsuarioId,
    string TecnicoNombre,
    DateTime UltimaActividad,
    string? UltimoMensaje,
    int TotalMensajes
);

public sealed record IniciarChatRequestDto(
    string TipoContexto,
    int ContextoId,
    string? MensajeInicial = null
);
