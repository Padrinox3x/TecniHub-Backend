using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Perfil;
using TecniHub.Application.DTOs.Servicios;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;

namespace TecniHub.Application.Services;

public sealed class ServicioService : IServicioService
{
    private readonly IPublicacionServicioRepository _servicioRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ITecnicoRepository _tecnicoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPerfilPublicoService? _perfilPublicoService;
    private readonly INotificacionPushService? _notificacionService;

    private static readonly IReadOnlyList<CategoriaDto> CategoriasDisponibles = new List<CategoriaDto>
    {
        new(1, "Refrigeradores", "Refrigeradores, neveras, congeladores y enfriadores", "kitchen"),
        new(2, "Lavadoras y Secadoras", "Lavadoras automáticas, semiautomáticas, secadoras y centros de lavado", "local_laundry_service"),
        new(3, "Microondas y Hornos", "Hornos de microondas, eléctricos y de convección", "microwave"),
        new(4, "Estufas y Parrillas", "Estufas de gas, eléctricas, parrillas y campanas extractoras", "stove"),
        new(5, "Aires Acondicionados", "Minisplits, aires de ventana y climatizadores", "ac_unit"),
        new(6, "Televisores y Pantallas", "Smart TVs, pantallas LED/OLED y monitores", "tv"),
        new(7, "Pequeños Electrodomésticos", "Licuadoras, cafeteras, tostadoras y freidoras de aire", "blender"),
        new(8, "Otros Electrodomésticos", "Otros aparatos del hogar o taller", "build")
    };

    public ServicioService(
        IPublicacionServicioRepository servicioRepository,
        IClienteRepository clienteRepository,
        ITecnicoRepository tecnicoRepository,
        IUnitOfWork unitOfWork,
        IPerfilPublicoService? perfilPublicoService = null,
        INotificacionPushService? notificacionService = null)
    {
        _servicioRepository = servicioRepository;
        _clienteRepository = clienteRepository;
        _tecnicoRepository = tecnicoRepository;
        _unitOfWork = unitOfWork;
        _perfilPublicoService = perfilPublicoService;
        _notificacionService = notificacionService;
    }

    public Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CategoriasDisponibles);
    }

    public async Task<ServicioCreadoDto> CrearServicioAsync(
        CrearServicioRequestDto request,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.TipoElectrodomestico))
            throw new ReglaNegocioInvalidaException("El tipo de electrodoméstico es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.DescripcionFalla))
            throw new ReglaNegocioInvalidaException("La descripción de la falla es obligatoria.");

        if (string.IsNullOrWhiteSpace(request.UbicacionAproximada))
            throw new ReglaNegocioInvalidaException("La ubicación aproximada es obligatoria.");

        var clienteId = await ResolverClienteIdAsync(request.ClienteId, usuarioIdAutenticado, cancellationToken);

        var titulo = string.IsNullOrWhiteSpace(request.Marca)
            ? $"Reparación de {request.TipoElectrodomestico}"
            : $"Reparación de {request.TipoElectrodomestico} {request.Marca}";

        var descripcionCompleta = ConstruirDescripcion(request);

        var ubicacionCompleta = string.IsNullOrWhiteSpace(request.Municipio)
            ? request.UbicacionAproximada.Trim()
            : $"{request.UbicacionAproximada.Trim()}, {request.Municipio.Trim()}";

        var servicio = new PublicacionServicio(
            clienteId: clienteId,
            titulo: titulo,
            tipoElectrodomestico: request.TipoElectrodomestico.Trim(),
            descripcionFalla: descripcionCompleta,
            ubicacionAproximada: ubicacionCompleta,
            imagenReferencia: request.ImagenReferencia
        );

        await _servicioRepository.AgregarAsync(servicio, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var folio = $"SRV-{DateTime.UtcNow:yyyyMMdd}-{servicio.Id:D4}";
        return new ServicioCreadoDto(servicio.Id.ToString(), folio, servicio.Estado);
    }

    public async Task<IReadOnlyList<ServicioDetalleDto>> ObtenerServiciosAsync(
        string? estado = null,
        int? clienteId = null,
        CancellationToken cancellationToken = default)
    {
        List<PublicacionServicio> lista;

        if (clienteId.HasValue)
        {
            lista = await _servicioRepository.ObtenerPorClienteIdAsync(clienteId.Value, cancellationToken);
        }
        else
        {
            lista = await _servicioRepository.ObtenerTodosAsync(incluirCompletados: true, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            lista = lista.Where(s => string.Equals(s.Estado, estado.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return lista.Select(MapearADetalle).ToList();
    }

    public async Task<ServicioDetalleDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var servicio = await _servicioRepository.ObtenerPorIdAsync(id, cancellationToken);
        return servicio is null ? null : MapearADetalle(servicio);
    }

    public async Task FinalizarServicioAsync(int id, int? usuarioIdAutenticado = null, CancellationToken cancellationToken = default)
    {
        var servicio = await _servicioRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la publicación de servicio con ID {id}.");

        if (usuarioIdAutenticado.HasValue && servicio.TecnicoId.HasValue)
        {
            var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioIdAutenticado.Value, cancellationToken);
            if (tecnico != null && tecnico.Id != servicio.TecnicoId.Value)
            {
                throw new UnauthorizedAccessException("Solo el técnico asignado puede marcar este servicio como finalizado.");
            }
        }

        servicio.FinalizarServicio();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_notificacionService != null)
        {
            await _notificacionService.NotificarCambioEstadoServicioAsync(
                servicio.Id,
                "Completado",
                usuarioIdAutenticado ?? 0,
                cancellationToken);
        }
    }

    public async Task<CierreServicioResponseDto> SolicitarCierreAsync(
        int id,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        var servicio = await _servicioRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la publicación de servicio con ID {id}.");

        if (usuarioIdAutenticado.HasValue && servicio.TecnicoId.HasValue)
        {
            var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioIdAutenticado.Value, cancellationToken);
            if (tecnico != null && tecnico.Id != servicio.TecnicoId.Value)
            {
                throw new UnauthorizedAccessException("Solo el técnico asignado puede solicitar el cierre de este servicio.");
            }
        }

        servicio.SolicitarCierre();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_notificacionService != null)
        {
            await _notificacionService.NotificarCambioEstadoServicioAsync(
                servicio.Id,
                "PendienteCierre",
                usuarioIdAutenticado ?? 0,
                cancellationToken);
        }

        return new CierreServicioResponseDto(
            ServicioId: servicio.Id,
            Estado: servicio.Estado,
            Mensaje: "Se ha solicitado el cierre del servicio. En espera de la confirmación y calificación del cliente.",
            CodigoColor: "Verde",
            ColorHex: "#22C55E",
            TextoIndicador: "Pendiente de confirmación",
            ResenaId: null,
            Calificacion: null,
            Comentario: null,
            FechaFinalizacion: DateTime.UtcNow
        );
    }

    public async Task<CierreServicioResponseDto> ConfirmarCierreYCalificarAsync(
        ConfirmarCierreYCalificarRequestDto request,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Calificacion < 1 || request.Calificacion > 5)
            throw new ReglaNegocioInvalidaException("Para cerrar el ciclo del servicio, la calificación debe ser de 1 a 5 estrellas.");

        var servicio = await _servicioRepository.ObtenerPorIdAsync(request.ServicioId, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la publicación de servicio con ID {request.ServicioId}.");

        if (usuarioIdAutenticado.HasValue)
        {
            var cliente = await _clienteRepository.ObtenerPorUsuarioIdAsync(usuarioIdAutenticado.Value, cancellationToken);
            if (cliente != null && cliente.Id != servicio.ClienteId)
            {
                throw new UnauthorizedAccessException("Solo el cliente que solicitó el servicio puede confirmar la finalización y emitir la calificación.");
            }
        }

        servicio.FinalizarServicio();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        string? resenaId = null;
        if (_perfilPublicoService != null && servicio.TecnicoId.HasValue)
        {
            var resena = await _perfilPublicoService.AgregarResenaAsync(
                servicio.TecnicoId.Value,
                new CrearResenaRequestDto(servicio.Id, request.Calificacion, request.Comentario),
                usuarioIdAutenticado,
                cancellationToken);
            resenaId = resena.Id;
        }

        if (_notificacionService != null)
        {
            await _notificacionService.NotificarCambioEstadoServicioAsync(
                servicio.Id,
                "Completado",
                usuarioIdAutenticado ?? 0,
                cancellationToken);
        }

        return new CierreServicioResponseDto(
            ServicioId: servicio.Id,
            Estado: "Completado",
            Mensaje: "El servicio ha sido completado exitosamente y la reseña ha sido registrada.",
            CodigoColor: "Azul",
            ColorHex: "#3B82F6",
            TextoIndicador: "Completado...",
            ResenaId: resenaId,
            Calificacion: request.Calificacion,
            Comentario: request.Comentario,
            FechaFinalizacion: DateTime.UtcNow
        );
    }

    public async Task<ServicioDetalleDto> CambiarEstadoAsync(
        int id,
        string nuevoEstado,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado))
            throw new ArgumentException("El nuevo estado es requerido.");

        var servicio = await _servicioRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la publicación de servicio con ID {id}.");

        var estadoNormalizado = nuevoEstado.Trim();
        servicio.CambiarEstado(estadoNormalizado);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_notificacionService != null)
        {
            await _notificacionService.NotificarCambioEstadoServicioAsync(
                servicio.Id,
                estadoNormalizado,
                usuarioIdAutenticado ?? 0,
                cancellationToken);
        }

        return MapearADetalle(servicio);
    }

    private async Task<int> ResolverClienteIdAsync(string clienteIdStr, int? usuarioId, CancellationToken cancellationToken)
    {
        if (usuarioId.HasValue)
        {
            var clienteAutenticado = await _clienteRepository.ObtenerPorUsuarioIdAsync(usuarioId.Value, cancellationToken);
            if (clienteAutenticado != null) return clienteAutenticado.Id;
        }

        if (int.TryParse(clienteIdStr, out var idInt) && idInt > 0)
        {
            return idInt;
        }

        // Si viene un UUID o texto no numérico, asignamos un cliente por defecto o 1
        return 1;
    }

    private static string ConstruirDescripcion(CrearServicioRequestDto request)
    {
        var detalles = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.Marca)) detalles.Add($"Marca: {request.Marca.Trim()}");
        if (!string.IsNullOrWhiteSpace(request.Modelo)) detalles.Add($"Modelo: {request.Modelo.Trim()}");
        if (!string.IsNullOrWhiteSpace(request.HorarioPreferido)) detalles.Add($"Horario preferido: {request.HorarioPreferido.Trim()}");
        if (request.FechaSolicitud.HasValue) detalles.Add($"Fecha preferida: {request.FechaSolicitud.Value:yyyy-MM-dd}");

        detalles.Add(request.DescripcionFalla.Trim());
        return string.Join(" | ", detalles);
    }

    private static ServicioDetalleDto MapearADetalle(PublicacionServicio s) =>
        new(
            Id: s.Id,
            CodigoFolio: $"SRV-{s.Id:D5}",
            ClienteId: s.ClienteId,
            ClienteNombre: s.Cliente != null ? $"{s.Cliente.Nombre} {s.Cliente.Apellidos}".Trim() : $"Cliente #{s.ClienteId}",
            TipoElectrodomestico: s.TipoElectrodomestico,
            DescripcionFalla: s.DescripcionFalla,
            UbicacionAproximada: s.UbicacionAproximada,
            Marca: null,
            Modelo: null,
            HorarioPreferido: null,
            ImagenReferencia: s.ImagenReferencia,
            Estado: s.Estado,
            TecnicoId: s.TecnicoId,
            TecnicoNombre: s.Tecnico?.NombreCompleto,
            FechaPublicacion: s.FechaPublicacion
        );
}
