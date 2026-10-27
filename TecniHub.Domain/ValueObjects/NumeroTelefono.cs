using System.Text.RegularExpressions;
using TecniHub.Domain.Exceptions;

namespace TecniHub.Domain.ValueObjects;

public record NumeroTelefono
{
    public string Valor { get; }

    public NumeroTelefono(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ReglaNegocioInvalidaException("El número de teléfono no puede estar vacío.");

        var numeroLimpio = valor.Replace(" ", "").Replace("-", "");

        var regex = new Regex(@"^\d{10}$");
        if (!regex.IsMatch(numeroLimpio))
            throw new ReglaNegocioInvalidaException($"El número de teléfono '{valor}' es inválido. Debe contener exactamente 10 dígitos.");

        Valor = numeroLimpio;
    }
}