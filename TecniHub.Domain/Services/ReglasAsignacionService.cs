using TecniHub.Domain.Entities;
using TecniHub.Domain.Exceptions;

namespace TecniHub.Domain.Services;

public class ReglasAsignacionService
{
    public void ValidarPostulacionTecnico(Tecnico tecnico, PublicacionServicio publicacion)
    {
        if (publicacion.Estado != "Pendiente")
        {
            throw new ReglaNegocioInvalidaException("Solo se pueden enviar cotizaciones a publicaciones pendientes.");
        }

        bool esAltaGama = publicacion.TipoElectrodomestico.Contains("Industrial", StringComparison.OrdinalIgnoreCase);

        if (esAltaGama && !tecnico.TieneNegocioFisico)
        {
            throw new ReglaNegocioInvalidaException("Los equipos industriales solo pueden ser atendidos por técnicos con negocio físico establecido.");
        }
    }
}