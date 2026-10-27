using System;

namespace TecniHub.Domain.Entities
{
    public class TecnicoEspecialidad
    {
        public Guid TecnicoId { get; set; }
        public short EspecialidadId { get; set; }

        // Columnas base de auditoria para la tabla de unión
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        // Propiedades de navegación
        public Tecnico Tecnico { get; set; } = null!;
        public Especialidad Especialidad { get; set; } = null!;
    }
}