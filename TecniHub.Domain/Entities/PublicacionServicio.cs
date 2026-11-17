using TecniHub.Domain.Exceptions;

namespace TecniHub.Domain.Entities;

public class PublicacionServicio
{
    public int Id { get; private set; }
    public int ClienteId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string TipoElectrodomestico { get; private set; } = string.Empty;
    public string DescripcionFalla { get; private set; } = string.Empty;
    public string UbicacionAproximada { get; private set; } = string.Empty;
    public DateTime FechaPublicacion { get; private set; }
    public string? ImagenReferencia { get; private set; }
    public string Estado { get; private set; } = "Pendiente"; // "Pendiente", "EnProceso", "Completado"
    public int? TecnicoId { get; private set; }

    public Cliente Cliente { get; private set; } = null!;
    public Tecnico? Tecnico { get; private set; }

    private PublicacionServicio() { }

    public PublicacionServicio(
        int clienteId,
        string titulo,
        string tipoElectrodomestico,
        string descripcionFalla,
        string ubicacionAproximada,
        string? imagenReferencia = null)
    {
        if (clienteId <= 0) throw new ArgumentException("El identificador del cliente es inválido.");
        if (string.IsNullOrWhiteSpace(titulo)) throw new ReglaNegocioInvalidaException("El título de la publicación es requerido.");
        if (string.IsNullOrWhiteSpace(tipoElectrodomestico)) throw new ReglaNegocioInvalidaException("El tipo de electrodoméstico es requerido.");
        if (string.IsNullOrWhiteSpace(descripcionFalla)) throw new ReglaNegocioInvalidaException("La descripción de la falla es requerida.");
        if (string.IsNullOrWhiteSpace(ubicacionAproximada)) throw new ReglaNegocioInvalidaException("La ubicación es requerida.");

        ClienteId = clienteId;
        Titulo = titulo.Trim();
        TipoElectrodomestico = tipoElectrodomestico.Trim();
        DescripcionFalla = descripcionFalla.Trim();
        UbicacionAproximada = ubicacionAproximada.Trim();
        ImagenReferencia = string.IsNullOrWhiteSpace(imagenReferencia) ? null : imagenReferencia.Trim();
        FechaPublicacion = DateTime.UtcNow;
        Estado = "Pendiente";
    }

    public PublicacionServicio(int clienteId, string tipoElectrodomestico, string descripcionFalla, string ubicacionAproximada)
        : this(clienteId, tipoElectrodomestico, tipoElectrodomestico, descripcionFalla, ubicacionAproximada)
    {
    }

    public void AceptarPorTecnico(int tecnicoId)
    {
        if (tecnicoId <= 0) throw new ArgumentException("El identificador del técnico es inválido.");
        if (!string.Equals(Estado, "Pendiente", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(Estado, "Abierto", StringComparison.OrdinalIgnoreCase))
        {
            throw new EstadoPublicacionInvalidoException(Estado);
        }

        TecnicoId = tecnicoId;
        Estado = "EnProceso";
    }

    public void FinalizarServicio()
    {
        if (!string.Equals(Estado, "EnProceso", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(Estado, "Aceptado", StringComparison.OrdinalIgnoreCase))
        {
            throw new ReglaNegocioInvalidaException("Solo se pueden finalizar servicios que se encuentren en proceso.");
        }

        Estado = "Completado";
    }

    public void AsignarImagenReferencia(string rutaImagen)
    {
        ImagenReferencia = rutaImagen?.Trim();
    }

    public void CambiarEstado(string nuevoEstado)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado)) throw new ArgumentException("El nuevo estado no es válido.");
        Estado = nuevoEstado.Trim();
    }
}
