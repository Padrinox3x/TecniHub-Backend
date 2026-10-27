using System;

namespace TecniHub.Domain.Entities
{
    public class ItinerarioTecnico
    {
        public Guid Id { get; set; }
        public Guid TecnicoId { get; set; }
        public Guid ServicioId { get; set; }
        public DateOnly FechaProgramada { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFinEstimada { get; set; }
        public string? NotasCita { get; set; }
        public string EstadoCita { get; set; } = "PROGRAMADO";

        // Columnas base de auditoria
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        // Propiedades de navegación
        public Tecnico Tecnico { get; set; } = null!;
        public PublicacionServicio Servicio { get; set; } = null!;
    }
}