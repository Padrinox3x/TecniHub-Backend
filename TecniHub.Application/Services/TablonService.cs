using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Tablon;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;
using TecniHub.Domain.Services;

namespace TecniHub.Application.Services;

public class TablonService : ITablonService
{
    private readonly IPublicacionServicioRepository _servicioRepository;
    private readonly IPublicacionPiezaRepository _piezaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ITecnicoRepository _tecnicoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ReglasAsignacionService _reglasAsignacionService;

    public TablonService(
        IPublicacionServicioRepository servicioRepository,
        IPublicacionPiezaRepository piezaRepository,
        IClienteRepository clienteRepository,
        ITecnicoRepository tecnicoRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        ReglasAsignacionService reglasAsignacionService)
    {
        _servicioRepository = servicioRepository;
        _piezaRepository = piezaRepository;
        _clienteRepository = clienteRepository;
        _tecnicoRepository = tecnicoRepository;
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _reglasAsignacionService = reglasAsignacionService;
    }

    public async Task<List<TablonItemDto>> ObtenerTablonAsync(
        TablonFiltroDto? filtro = null,
        CancellationToken cancellationToken = default)
    {
        var incluirCompletados = filtro?.IncluirCompletados ?? false;
        var tipoFiltro = filtro?.Tipo?.Trim().ToUpperInvariant();

        var items = new List<TablonItemDto>();

        // 1. Cargar Servicios si no se filtró expresamente por piezas
        if (tipoFiltro != "PIEZA")
        {
            var servicios = await _servicioRepository.ObtenerTodosAsync(incluirCompletados, cancellationToken);
            items.AddRange(servicios.Select(MapearServicioATablonItem));
        }

        // 2. Cargar Piezas si no se filtró expresamente por servicios
        if (tipoFiltro != "SERVICIO")
        {
            var piezas = await _piezaRepository.ObtenerTodosAsync(incluirCompletados, cancellationToken);
            items.AddRange(piezas.Select(MapearPiezaATablonItem));
        }

        // 3. Aplicar filtros adicionales en memoria
        var resultado = items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filtro?.Color))
        {
            resultado = resultado.Where(i => string.Equals(i.CodigoColor, filtro.Color, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filtro?.Estado))
        {
            resultado = resultado.Where(i => string.Equals(i.Estado, filtro.Estado, StringComparison.OrdinalIgnoreCase));
        }

        return resultado.OrderByDescending(i => i.FechaPublicacion).ToList();
    }

    public async Task<TablonItemDto> ObtenerPublicacionServicioPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var servicio = await _servicioRepository.ObtenerPorIdAsync(id, cancellationToken);
        if (servicio is null)
        {
            throw new KeyNotFoundException($"No se encontró la publicación de servicio con ID {id}.");
        }

        return MapearServicioATablonItem(servicio);
    }

    public async Task<TablonItemDto> ObtenerPublicacionPiezaPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var pieza = await _piezaRepository.ObtenerPorIdAsync(id, cancellationToken);
        if (pieza is null)
        {
            throw new KeyNotFoundException($"No se encontró la publicación de pieza con ID {id}.");
        }

        return MapearPiezaATablonItem(pieza);
    }

    public async Task<TablonItemDto> CrearPublicacionServicioAsync(
        int usuarioId,
        CrearPublicacionServicioRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var cliente = await _clienteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
        if (cliente is null)
        {
            throw new InvalidOperationException("Solo los usuarios con rol de Cliente pueden publicar solicitudes de servicio en el tablón.");
        }

        var publicacion = new PublicacionServicio(
            cliente.Id,
            request.Titulo,
            request.TipoElectrodomestico,
            request.DescripcionFalla,
            request.UbicacionAproximada,
            request.ImagenReferencia);

        await _servicioRepository.AgregarAsync(publicacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recargar con navegación
        var creada = await _servicioRepository.ObtenerPorIdAsync(publicacion.Id, cancellationToken);
        return MapearServicioATablonItem(creada ?? publicacion);
    }

    public async Task<TablonItemDto> CrearPublicacionPiezaAsync(
        int usuarioId,
        CrearPublicacionPiezaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
        if (tecnico is null)
        {
            throw new InvalidOperationException("Solo los técnicos pueden publicar refacciones o piezas en el tablón.");
        }

        var publicacion = new PublicacionPieza(
            tecnico.Id,
            request.NombrePieza,
            request.CategoriaAparato,
            request.CondicionPieza,
            request.DatosContacto,
            request.TipoTransaccion,
            request.Precio,
            request.DescripcionDetalle,
            request.Fotografia);

        await _piezaRepository.AgregarAsync(publicacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var creada = await _piezaRepository.ObtenerPorIdAsync(publicacion.Id, cancellationToken);
        return MapearPiezaATablonItem(creada ?? publicacion);
    }

    public async Task<TablonItemDto> AceptarServicioAsync(
        int usuarioId,
        int publicacionId,
        CancellationToken cancellationToken = default)
    {
        var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
        if (tecnico is null)
        {
            throw new InvalidOperationException("Solo los técnicos registrados pueden aceptar servicios del tablón.");
        }

        var servicio = await _servicioRepository.ObtenerPorIdAsync(publicacionId, cancellationToken);
        if (servicio is null)
        {
            throw new KeyNotFoundException($"No se encontró la publicación de servicio con ID {publicacionId}.");
        }

        // Validar reglas de dominio (ej. equipos industriales solo con negocio físico)
        _reglasAsignacionService.ValidarPostulacionTecnico(tecnico, servicio);

        servicio.AceptarPorTecnico(tecnico.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var actualizado = await _servicioRepository.ObtenerPorIdAsync(publicacionId, cancellationToken);
        return MapearServicioATablonItem(actualizado ?? servicio);
    }

    public async Task<TablonItemDto> FinalizarServicioAsync(
        int usuarioId,
        int publicacionId,
        CancellationToken cancellationToken = default)
    {
        var servicio = await _servicioRepository.ObtenerPorIdAsync(publicacionId, cancellationToken);
        if (servicio is null)
        {
            throw new KeyNotFoundException($"No se encontró la publicación de servicio con ID {publicacionId}.");
        }

        var cliente = await _clienteRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
        var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);

        // Validar que quien finaliza sea el cliente autor o el técnico asignado
        bool esClienteAutor = cliente is not null && servicio.ClienteId == cliente.Id;
        bool esTecnicoAsignado = tecnico is not null && servicio.TecnicoId == tecnico.Id;

        if (!esClienteAutor && !esTecnicoAsignado)
        {
            throw new UnauthorizedAccessException("Solo el cliente que solicitó el servicio o el técnico asignado pueden marcarlo como finalizado.");
        }

        servicio.FinalizarServicio();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var actualizado = await _servicioRepository.ObtenerPorIdAsync(publicacionId, cancellationToken);
        return MapearServicioATablonItem(actualizado ?? servicio);
    }

    public async Task<TablonItemDto> FinalizarPiezaAsync(
        int usuarioId,
        int publicacionId,
        CancellationToken cancellationToken = default)
    {
        var pieza = await _piezaRepository.ObtenerPorIdAsync(publicacionId, cancellationToken);
        if (pieza is null)
        {
            throw new KeyNotFoundException($"No se encontró la publicación de pieza con ID {publicacionId}.");
        }

        var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioId, cancellationToken);
        if (tecnico is null || pieza.TecnicoId != tecnico.Id)
        {
            throw new UnauthorizedAccessException("Solo el técnico que publicó la refacción puede marcarla como completada.");
        }

        pieza.FinalizarTransaccion();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var actualizado = await _piezaRepository.ObtenerPorIdAsync(publicacionId, cancellationToken);
        return MapearPiezaATablonItem(actualizado ?? pieza);
    }

    private static TablonItemDto MapearServicioATablonItem(PublicacionServicio s)
    {
        var visual = TablonColorEvaluator.EvaluarServicio(s);
        var autorNombre = s.Cliente is not null
            ? $"@{s.Cliente.Nombre} {s.Cliente.Apellidos.FirstOrDefault()}."
            : "@Cliente";

        return new TablonItemDto
        {
            Id = s.Id,
            TipoPublicacion = "SERVICIO",
            Titulo = !string.IsNullOrWhiteSpace(s.Titulo) ? s.Titulo : s.TipoElectrodomestico,
            Descripcion = s.DescripcionFalla,
            AutorId = s.ClienteId,
            AutorNombre = autorNombre,
            AutorRol = "CLIENTE",
            Estado = s.Estado,
            Modalidad = "Reparacion",
            CodigoColor = visual.CodigoColor,
            ClaseCssBorde = visual.ClaseCssBorde,
            ColorHex = visual.ColorHex,
            EsCompletado = visual.EsCompletado,
            TextoIndicador = visual.TextoIndicador,
            Ubicacion = s.UbicacionAproximada,
            ImagenUrl = s.ImagenReferencia,
            FechaPublicacion = s.FechaPublicacion,
            TecnicoAsignadoId = s.TecnicoId,
            TecnicoAsignadoNombre = s.Tecnico?.NombreCompleto
        };
    }

    private static TablonItemDto MapearPiezaATablonItem(PublicacionPieza p)
    {
        var visual = TablonColorEvaluator.EvaluarPieza(p);
        var autorNombre = p.Tecnico is not null
            ? $"@{p.Tecnico.NombreCompleto}"
            : "@Tecnico";

        return new TablonItemDto
        {
            Id = p.Id,
            TipoPublicacion = "PIEZA",
            Titulo = p.NombrePieza,
            Descripcion = !string.IsNullOrWhiteSpace(p.DescripcionDetalle) ? p.DescripcionDetalle : $"{p.CategoriaAparato} - {p.CondicionPieza}",
            AutorId = p.TecnicoId,
            AutorNombre = autorNombre,
            AutorRol = "TECNICO",
            Estado = p.EstadoDisponibilidad,
            Modalidad = p.TipoTransaccion,
            CodigoColor = visual.CodigoColor,
            ClaseCssBorde = visual.ClaseCssBorde,
            ColorHex = visual.ColorHex,
            EsCompletado = visual.EsCompletado,
            TextoIndicador = visual.TextoIndicador,
            Precio = p.Precio,
            ImagenUrl = p.Fotografia,
            FechaPublicacion = p.FechaPublicacion
        };
    }
}
