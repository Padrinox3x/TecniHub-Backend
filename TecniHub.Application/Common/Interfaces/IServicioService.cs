using TecniHub.Application.DTOs.Servicios;

namespace TecniHub.Application.Common.Interfaces;

public interface IServicioService
{
    Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken = default);

    Task<ServicioCreadoDto> CrearServicioAsync(
        CrearServicioRequestDto request,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServicioDetalleDto>> ObtenerServiciosAsync(
        string? estado = null,
        int? clienteId = null,
        CancellationToken cancellationToken = default);

    Task<ServicioDetalleDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task FinalizarServicioAsync(
        int id,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);

    Task<CierreServicioResponseDto> SolicitarCierreAsync(
        int id,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);

    Task<CierreServicioResponseDto> ConfirmarCierreYCalificarAsync(
        ConfirmarCierreYCalificarRequestDto request,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);

    Task<ServicioDetalleDto> CambiarEstadoAsync(
        int id,
        string nuevoEstado,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default);
}
