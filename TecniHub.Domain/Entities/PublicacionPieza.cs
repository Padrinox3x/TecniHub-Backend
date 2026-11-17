using TecniHub.Domain.Exceptions;

namespace TecniHub.Domain.Entities;

public class PublicacionPieza
{
    public int Id { get; private set; }
    public int TecnicoId { get; private set; }
    public string NombrePieza { get; private set; } = string.Empty;
    public string CategoriaAparato { get; private set; } = string.Empty;
    public string CondicionPieza { get; private set; } = string.Empty;
    public string? DescripcionDetalle { get; private set; }
    public decimal? Precio { get; private set; }
    public string DatosContacto { get; private set; } = string.Empty;
    public string? Fotografia { get; private set; }
    public string TipoTransaccion { get; private set; } = "Venta"; // "Venta", "Intercambio", "VentaOIntercambio"
    public string EstadoDisponibilidad { get; private set; } = "Disponible"; // "Disponible", "Completado"
    public DateTime FechaPublicacion { get; private set; }

    public Tecnico Tecnico { get; private set; } = null!;

    private PublicacionPieza() { }

    public PublicacionPieza(
        int tecnicoId,
        string nombrePieza,
        string categoriaAparato,
        string condicionPieza,
        string datosContacto,
        string tipoTransaccion = "Venta",
        decimal? precio = null,
        string? descripcionDetalle = null,
        string? fotografia = null)
    {
        if (tecnicoId <= 0) throw new ArgumentException("El identificador del técnico es inválido.");
        if (string.IsNullOrWhiteSpace(nombrePieza)) throw new ReglaNegocioInvalidaException("El nombre de la pieza es requerido.");
        if (string.IsNullOrWhiteSpace(categoriaAparato)) throw new ReglaNegocioInvalidaException("La categoría del aparato es requerida.");
        if (string.IsNullOrWhiteSpace(condicionPieza)) throw new ReglaNegocioInvalidaException("La condición de la pieza es requerida.");
        if (string.IsNullOrWhiteSpace(datosContacto)) throw new ReglaNegocioInvalidaException("Los datos de contacto son requeridos.");

        ValidarTipoTransaccion(tipoTransaccion);

        if (precio.HasValue && precio.Value < 0)
        {
            throw new PrecioInvalidoException(precio.Value);
        }

        TecnicoId = tecnicoId;
        NombrePieza = nombrePieza.Trim();
        CategoriaAparato = categoriaAparato.Trim();
        CondicionPieza = condicionPieza.Trim();
        DatosContacto = datosContacto.Trim();
        TipoTransaccion = tipoTransaccion.Trim();
        Precio = precio;
        DescripcionDetalle = string.IsNullOrWhiteSpace(descripcionDetalle) ? null : descripcionDetalle.Trim();
        Fotografia = string.IsNullOrWhiteSpace(fotografia) ? null : fotografia.Trim();
        EstadoDisponibilidad = "Disponible";
        FechaPublicacion = DateTime.UtcNow;
    }

    public void FinalizarTransaccion()
    {
        if (string.Equals(EstadoDisponibilidad, "Completado", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        EstadoDisponibilidad = "Completado";
    }

    public void AsignarPrecio(decimal nuevoPrecio)
    {
        if (nuevoPrecio < 0) throw new PrecioInvalidoException(nuevoPrecio);
        Precio = nuevoPrecio;
    }

    public void ActualizarDetalles(string? descripcion, string? fotografia)
    {
        DescripcionDetalle = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        Fotografia = string.IsNullOrWhiteSpace(fotografia) ? null : fotografia.Trim();
    }

    public void CambiarEstadoDisponibilidad(string nuevoEstado)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado)) throw new ArgumentException("El estado de disponibilidad es inválido.");
        EstadoDisponibilidad = nuevoEstado.Trim();
    }

    public void CambiarTipoTransaccion(string nuevoTipo)
    {
        ValidarTipoTransaccion(nuevoTipo);
        TipoTransaccion = nuevoTipo.Trim();
    }

    private static void ValidarTipoTransaccion(string tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            throw new ReglaNegocioInvalidaException("El tipo de transacción es requerido.");
        }

        var tipoNormalizado = tipo.Trim().ToUpperInvariant();
        if (tipoNormalizado != "VENTA" && tipoNormalizado != "INTERCAMBIO" && tipoNormalizado != "VENTAOINTERCAMBIO" && tipoNormalizado != "AMBOS")
        {
            throw new ReglaNegocioInvalidaException($"Tipo de transacción '{tipo}' no es válido. Opciones permitidas: 'Venta', 'Intercambio', 'VentaOIntercambio'.");
        }
    }
}
