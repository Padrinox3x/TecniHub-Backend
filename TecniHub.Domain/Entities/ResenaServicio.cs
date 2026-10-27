using System;

namespace TecniHub.Domain.Entities
{
    public class ResenaServicio
    {
        public Guid Id { get; set; }
        public Guid ServicioId { get; set; }
        public Guid ClienteId { get; set; }
        public Guid TecnicoId { get; set; }
        public short Calificacion { get; set; }
        public string? Comentario { get; set; }

        // Columnas base de auditoria
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        // Propiedades de navegación
        public PublicacionServicio Servicio { get; set; } = null!;
        public Cliente Cliente { get; set; } = null!;
        public Tecnico Tecnico { get; set; } = null!;
    }
}