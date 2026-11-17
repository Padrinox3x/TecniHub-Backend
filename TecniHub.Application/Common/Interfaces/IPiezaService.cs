using TecniHub.Application.DTOs.Piezas;

namespace TecniHub.Application.Common.Interfaces;

public interface IPiezaService
{
    Task<string> PublicarPiezaAsync(
        PublicarPiezaRequestDto request,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PiezaDto>> BuscarPiezasAsync(
        BuscarPiezasRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PiezaDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task CambiarDisponibilidadAsync(
        int id,
        string nuevoEstado,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);
}
