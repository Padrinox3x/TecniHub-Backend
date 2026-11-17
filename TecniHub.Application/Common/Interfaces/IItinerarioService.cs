using TecniHub.Application.DTOs.Itinerarios;

namespace TecniHub.Application.Common.Interfaces;

public interface IItinerarioService
{
    Task<ItinerarioProgramadoDto> ProgramarItinerarioAsync(
        ProgramarItinerarioRequestDto request,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItinerarioItemDto>> ObtenerItinerarioAsync(
        int? tecnicoId = null,
        int? usuarioIdAutenticado = null,
        bool incluirCompletados = false,
        CancellationToken cancellationToken = default);

    Task FinalizarServicioAgendadoAsync(
        int servicioId,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);
}
