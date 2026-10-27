namespace TecniHub.Domain.Entities;

public class PublicacionPieza
{
    public int Id { get; private set; }
    public int TecnicoId { get; private set; }
    public string NombrePieza { get; private set; }
    public string CategoriaAparato { get; private set; }
    public string CondicionPieza { get; private set; }
    public string? DescripcionDetalle { get; private set; }
    public decimal? Precio { get; private set; }
    public string DatosContacto { get; private set; }
    public string? Fotografia { get; private set; }
    public string EstadoDisponibilidad { get; private set; }
    public DateTime FechaPublicacion { get; private set; }

    public Tecnico Tecnico { get; private set; } = null!;

    private PublicacionPieza() { }

    public PublicacionPieza(int tecnicoId, string nombrePieza, string categoriaAparato, string condicionPieza, string datosContacto)
    {
        if (tecnicoId <= 0) throw new ArgumentException("El identificador del técnico es inválido.");
        if (string.IsNullOrWhiteSpace(nombrePieza)) throw new ArgumentException("El nombre de la pieza es requerido.");
        if (string.IsNullOrWhiteSpace(categoriaAparato)) throw new ArgumentException("La categoría del aparato es requerida.");
        if (string.IsNullOrWhiteSpace(condicionPieza)) throw new ArgumentException("La condición de la pieza es requerida.");
        if (string.IsNullOrWhiteSpace(datosContacto)) throw new ArgumentException("Los datos de contacto son requeridos.");

        TecnicoId = tecnicoId;
        NombrePieza = nombrePieza;
        CategoriaAparato = categoriaAparato;
        CondicionPieza = condicionPieza;
        DatosContacto = datosContacto;

        EstadoDisponibilidad = "Disponible";
        FechaPublicacion = DateTime.UtcNow;
    }

    public void AsignarPrecio(decimal nuevoPrecio)
    {
        if (nuevoPrecio < 0) throw new ArgumentException("El precio no puede ser negativo.");
        Precio = nuevoPrecio;
    }

    public void ActualizarDetalles(string? descripcion, string? fotografia)
    {
        DescripcionDetalle = descripcion;
        Fotografia = fotografia;
    }

    public void CambiarEstadoDisponibilidad(string nuevoEstado)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado)) throw new ArgumentException("El estado de disponibilidad es inválido.");
        EstadoDisponibilidad = nuevoEstado;
    }
}