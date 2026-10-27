namespace TecniHub.Domain.Entities;

public class PublicacionServicio
{
    public int Id { get; private set; }
    public int ClienteId { get; private set; }
    public string TipoElectrodomestico { get; private set; }
    public string DescripcionFalla { get; private set; }
    public string UbicacionAproximada { get; private set; }
    public DateTime FechaPublicacion { get; private set; }
    public string? ImagenReferencia { get; private set; }
    public string Estado { get; private set; }

    public Cliente Cliente { get; private set; } = null!;

    private PublicacionServicio() { }

    public PublicacionServicio(int clienteId, string tipoElectrodomestico, string descripcionFalla, string ubicacionAproximada)
    {
        if (clienteId <= 0) throw new ArgumentException("El identificador del cliente es inválido.");
        if (string.IsNullOrWhiteSpace(tipoElectrodomestico)) throw new ArgumentException("El tipo de electrodoméstico es requerido.");
        if (string.IsNullOrWhiteSpace(descripcionFalla)) throw new ArgumentException("La descripción de la falla es requerida.");
        if (string.IsNullOrWhiteSpace(ubicacionAproximada)) throw new ArgumentException("La ubicación es requerida.");

        ClienteId = clienteId;
        TipoElectrodomestico = tipoElectrodomestico;
        DescripcionFalla = descripcionFalla;
        UbicacionAproximada = ubicacionAproximada;

        FechaPublicacion = DateTime.UtcNow; 
        Estado = "Pendiente"; 
    }

    public void AsignarImagenReferencia(string rutaImagen)
    {
        ImagenReferencia = rutaImagen;
    }

    public void CambiarEstado(string nuevoEstado)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado)) throw new ArgumentException("El nuevo estado no es válido.");
        Estado = nuevoEstado;
    }
}