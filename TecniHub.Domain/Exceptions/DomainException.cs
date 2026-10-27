namespace TecniHub.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public class PrecioInvalidoException : DomainException
{
    public PrecioInvalidoException(decimal precio)
        : base($"El precio {precio} no es válido. No puede ser un valor negativo.") { }
}

public class EstadoPublicacionInvalidoException : DomainException
{
    public EstadoPublicacionInvalidoException(string estadoActual)
        : base($"No se puede modificar la publicación porque se encuentra en estado: {estadoActual}.") { }
}

public class CorreoInvalidoException : DomainException
{
    public CorreoInvalidoException(string mensaje) : base(mensaje) { }
}

public class ReglaNegocioInvalidaException : DomainException
{
    public ReglaNegocioInvalidaException(string mensaje) : base(mensaje) { }
}