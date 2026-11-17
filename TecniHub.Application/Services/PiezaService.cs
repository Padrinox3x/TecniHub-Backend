using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Piezas;
using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;

namespace TecniHub.Application.Services;

public sealed class PiezaService : IPiezaService
{
    private readonly IPublicacionPiezaRepository _piezaRepository;
    private readonly ITecnicoRepository _tecnicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly Dictionary<int, string> CategoriasNombres = new()
    {
        { 1, "Refrigeradores" },
        { 2, "Lavadoras y Secadoras" },
        { 3, "Microondas y Hornos" },
        { 4, "Estufas y Parrillas" },
        { 5, "Aires Acondicionados" },
        { 6, "Televisores y Pantallas" },
        { 7, "Pequeños Electrodomésticos" },
        { 8, "Otros Electrodomésticos" }
    };

    public PiezaService(
        IPublicacionPiezaRepository piezaRepository,
        ITecnicoRepository tecnicoRepository,
        IUnitOfWork unitOfWork)
    {
        _piezaRepository = piezaRepository;
        _tecnicoRepository = tecnicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> PublicarPiezaAsync(
        PublicarPiezaRequestDto request,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ReglaNegocioInvalidaException("El nombre de la pieza es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Descripcion))
            throw new ReglaNegocioInvalidaException("La descripción de la pieza es obligatoria.");

        if (string.IsNullOrWhiteSpace(request.MedioContacto))
            throw new ReglaNegocioInvalidaException("El medio de contacto es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.UbicacionEntrega))
            throw new ReglaNegocioInvalidaException("La ubicación de entrega es obligatoria.");

        var tecnicoId = await ResolverTecnicoIdAsync(request.TecnicoId, usuarioIdAutenticado, cancellationToken);

        var modalidadNormalizada = NormalizarModalidad(request.Modalidad);

        var categoriaNombre = CategoriasNombres.TryGetValue(request.CategoriaId, out var cat)
            ? cat
            : $"Categoría {request.CategoriaId}";

        var descripcionDetalle = ConstruirDetalles(request);

        var pieza = new PublicacionPieza(
            tecnicoId: tecnicoId,
            nombrePieza: request.Nombre.Trim(),
            categoriaAparato: categoriaNombre,
            condicionPieza: request.Condicion.Trim(),
            datosContacto: $"{request.MedioContacto.Trim()} | Entrega: {request.UbicacionEntrega.Trim()}",
            tipoTransaccion: modalidadNormalizada,
            precio: request.Precio,
            descripcionDetalle: descripcionDetalle,
            fotografia: request.Fotografia
        );

        await _piezaRepository.AgregarAsync(pieza, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return pieza.Id.ToString();
    }

    public async Task<IReadOnlyList<PiezaDto>> BuscarPiezasAsync(
        BuscarPiezasRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var piezas = await _piezaRepository.ObtenerTodosAsync(incluirCompletados: false, cancellationToken);

        var query = piezas.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Texto))
        {
            var texto = request.Texto.Trim();
            query = query.Where(p =>
                p.NombrePieza.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                (p.DescripcionDetalle != null && p.DescripcionDetalle.Contains(texto, StringComparison.OrdinalIgnoreCase)) ||
                p.CategoriaAparato.Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        if (request.CategoriaId.HasValue && CategoriasNombres.TryGetValue(request.CategoriaId.Value, out var catNombre))
        {
            query = query.Where(p => p.CategoriaAparato.Contains(catNombre, StringComparison.OrdinalIgnoreCase)
                                  || p.CategoriaAparato.Contains(request.CategoriaId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(request.Condicion))
        {
            var condicion = request.Condicion.Trim().ToUpperInvariant();
            query = query.Where(p => p.CondicionPieza.Trim().ToUpperInvariant() == condicion);
        }

        if (!string.IsNullOrWhiteSpace(request.Modalidad))
        {
            var mod = NormalizarModalidad(request.Modalidad).ToUpperInvariant();
            query = query.Where(p => p.TipoTransaccion.Trim().ToUpperInvariant() == mod);
        }

        if (request.PrecioMaximo.HasValue)
        {
            query = query.Where(p => !p.Precio.HasValue || p.Precio.Value <= request.PrecioMaximo.Value);
        }

        return query.Select(MapearAPiezaDto).ToList();
    }

    public async Task<PiezaDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var pieza = await _piezaRepository.ObtenerPorIdAsync(id, cancellationToken);
        return pieza is null ? null : MapearAPiezaDto(pieza);
    }

    public async Task CambiarDisponibilidadAsync(
        int id,
        string nuevoEstado,
        int? usuarioIdAutenticado = null,
        CancellationToken cancellationToken = default)
    {
        var pieza = await _piezaRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la publicación de pieza con ID {id}.");

        if (usuarioIdAutenticado.HasValue)
        {
            var tecnico = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioIdAutenticado.Value, cancellationToken);
            if (tecnico != null && tecnico.Id != pieza.TecnicoId)
            {
                throw new UnauthorizedAccessException("Solo el técnico que publicó esta refacción puede modificar su disponibilidad.");
            }
        }

        if (string.Equals(nuevoEstado, "Completado", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(nuevoEstado, "NoDisponible", StringComparison.OrdinalIgnoreCase))
        {
            pieza.FinalizarTransaccion();
        }
        else
        {
            pieza.CambiarEstadoDisponibilidad(nuevoEstado);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> ResolverTecnicoIdAsync(string tecnicoIdStr, int? usuarioId, CancellationToken cancellationToken)
    {
        if (usuarioId.HasValue)
        {
            var tecnicoAutenticado = await _tecnicoRepository.ObtenerPorUsuarioIdAsync(usuarioId.Value, cancellationToken);
            if (tecnicoAutenticado != null) return tecnicoAutenticado.Id;
        }

        if (int.TryParse(tecnicoIdStr, out var idInt) && idInt > 0)
        {
            return idInt;
        }

        // Si viene un UUID o texto no numérico, asignamos el primer técnico disponible o 1
        return 1;
    }

    private static string NormalizarModalidad(string modalidad)
    {
        var m = (modalidad ?? "VENTA").Trim().ToUpperInvariant();
        return m switch
        {
            "INTERCAMBIO" => "Intercambio",
            "VENTA_E_INTERCAMBIO" or "VENTAOINTERCAMBIO" or "AMBOS" => "Ambos",
            _ => "Venta"
        };
    }

    private static string ConstruirDetalles(PublicarPiezaRequestDto r)
    {
        var partes = new List<string> { r.Descripcion.Trim() };
        if (!string.IsNullOrWhiteSpace(r.PiezaIntercambioDeseada))
        {
            partes.Add($"Pieza deseada a cambio: {r.PiezaIntercambioDeseada.Trim()}");
        }
        return string.Join(" | ", partes);
    }

    private static PiezaDto MapearAPiezaDto(PublicacionPieza p)
    {
        var categoriaId = 1;
        foreach (var kv in CategoriasNombres)
        {
            if (p.CategoriaAparato.Contains(kv.Value, StringComparison.OrdinalIgnoreCase))
            {
                categoriaId = kv.Key;
                break;
            }
        }

        var medioContacto = p.DatosContacto;
        var ubicacionEntrega = string.Empty;
        if (p.DatosContacto.Contains("| Entrega:"))
        {
            var split = p.DatosContacto.Split("| Entrega:", 2);
            medioContacto = split[0].Trim();
            ubicacionEntrega = split[1].Trim();
        }

        string? intercambioDeseado = null;
        var desc = p.DescripcionDetalle ?? string.Empty;
        if (desc.Contains("| Pieza deseada a cambio:"))
        {
            var split = desc.Split("| Pieza deseada a cambio:", 2);
            desc = split[0].Trim();
            intercambioDeseado = split[1].Trim();
        }

        var modalidadFrontend = p.TipoTransaccion.ToUpperInvariant() switch
        {
            "INTERCAMBIO" => "INTERCAMBIO",
            "AMBOS" or "VENTAOINTERCAMBIO" => "VENTA_E_INTERCAMBIO",
            _ => "VENTA"
        };

        return new PiezaDto(
            Id: p.Id.ToString(),
            CodigoFolio: $"PIEZA-{p.Id:D5}",
            CategoriaId: categoriaId,
            Nombre: p.NombrePieza,
            Descripcion: desc,
            Condicion: p.CondicionPieza,
            Modalidad: modalidadFrontend,
            Precio: p.Precio,
            PiezaIntercambioDeseada: intercambioDeseado,
            MedioContacto: medioContacto,
            UbicacionEntrega: ubicacionEntrega,
            Fotografia: p.Fotografia,
            EstadoDisponibilidad: p.EstadoDisponibilidad,
            TecnicoId: p.TecnicoId,
            FechaPublicacion: p.FechaPublicacion
        );
    }
}
