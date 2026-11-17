using TecniHub.Domain.Entities;

namespace TecniHub.Domain.Services;

public record InformacionVisualColor(
    string CodigoColor,
    string ClaseCssBorde,
    string ColorHex,
    bool EsCompletado,
    string? TextoIndicador);

public static class TablonColorEvaluator
{
    public static InformacionVisualColor EvaluarServicio(PublicacionServicio servicio)
    {
        var estado = servicio.Estado.Trim().ToUpperInvariant();

        return estado switch
        {
            "COMPLETADO" or "FINALIZADO" => new InformacionVisualColor(
                CodigoColor: "Azul",
                ClaseCssBorde: "border-blue-500",
                ColorHex: "#3B82F6",
                EsCompletado: true,
                TextoIndicador: "Completado..."),

            "ENPROCESO" or "EN_PROCESO" or "ACEPTADO" => new InformacionVisualColor(
                CodigoColor: "Verde",
                ClaseCssBorde: "border-green-500",
                ColorHex: "#22C55E",
                EsCompletado: false,
                TextoIndicador: null),

            _ => new InformacionVisualColor(
                CodigoColor: "Rojo",
                ClaseCssBorde: "border-red-500",
                ColorHex: "#EF4444",
                EsCompletado: false,
                TextoIndicador: null)
        };
    }

    public static InformacionVisualColor EvaluarPieza(PublicacionPieza pieza)
    {
        var estado = pieza.EstadoDisponibilidad.Trim().ToUpperInvariant();

        if (estado is "COMPLETADO" or "VENDIDO" or "INTERCAMBIADO" or "FINALIZADO")
        {
            return new InformacionVisualColor(
                CodigoColor: "AzulMarino",
                ClaseCssBorde: "border-blue-700",
                ColorHex: "#1D4ED8",
                EsCompletado: true,
                TextoIndicador: "Completado...");
        }

        var tipo = pieza.TipoTransaccion.Trim().ToUpperInvariant();

        return tipo switch
        {
            "INTERCAMBIO" => new InformacionVisualColor(
                CodigoColor: "Naranja",
                ClaseCssBorde: "border-orange-500",
                ColorHex: "#F97316",
                EsCompletado: false,
                TextoIndicador: null),

            "VENTAOINTERCAMBIO" or "AMBOS" => new InformacionVisualColor(
                CodigoColor: "AmarilloNaranja",
                ClaseCssBorde: "border-bicolor-yellow-orange",
                ColorHex: "#EAB308,#F97316",
                EsCompletado: false,
                TextoIndicador: null),

            _ => new InformacionVisualColor(
                CodigoColor: "Amarillo",
                ClaseCssBorde: "border-yellow-500",
                ColorHex: "#EAB308",
                EsCompletado: false,
                TextoIndicador: null)
        };
    }
}
