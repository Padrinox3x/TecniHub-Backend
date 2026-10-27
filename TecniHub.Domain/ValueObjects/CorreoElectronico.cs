using System.Text.RegularExpressions;
using TecniHub.Domain.Exceptions;

namespace TecniHub.Domain.ValueObjects;

public record CorreoElectronico
{
    public string Valor { get; }

    public CorreoElectronico(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new CorreoInvalidoException("El correo no puede estar vacío.");

        var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!regex.IsMatch(valor))
            throw new CorreoInvalidoException($"El formato del correo '{valor}' es inválido.");

        Valor = valor;
    }
}