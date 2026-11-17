namespace TecniHub.Application.DTOs.Itinerarios;

public sealed record ProgramarItinerarioRequestDto(
    string ServicioId,
    string TecnicoId,
    string? ActorId,
    string Fecha,
    string HoraInicio,
    string HoraFinEstimada,
    string? Notas = null
);

public sealed record ItinerarioProgramadoDto(
    string ServicioId,
    string Fecha,
    string HoraInicio,
    string HoraFinEstimada,
    string Estado,
    string? Advertencia = null
);

public sealed record ItinerarioItemDto(
    int Id,
    int ServicioId,
    string CodigoFolio,
    int TecnicoId,
    string? TecnicoNombre,
    string TituloServicio,
    string TipoElectrodomestico,
    string Ubicacion,
    string FechaProgramada,
    string HoraInicio,
    string HoraFinEstimada,
    string? Notas,
    string Estado,
    bool EsCompletado
);
