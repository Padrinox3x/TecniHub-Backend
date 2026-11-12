using System.ComponentModel.DataAnnotations;

namespace TecniHub.Application.DTOs.Sprint3;

public sealed class ProgramarItinerarioRequestDto
{
    [Required] public Guid ServicioId { get; init; }
    [Required] public Guid TecnicoId { get; init; }
    [Required] public Guid ActorId { get; init; }
    [Required] public DateOnly? Fecha { get; init; }
    [Required] public TimeOnly? HoraInicio { get; init; }
    [Required] public TimeOnly? HoraFinEstimada { get; init; }
    [StringLength(2000)] public string? Notas { get; init; }
}

public sealed record ItinerarioProgramadoDto(Guid ServicioId, DateOnly Fecha,
    TimeOnly HoraInicio, TimeOnly HoraFinEstimada, string Estado);
