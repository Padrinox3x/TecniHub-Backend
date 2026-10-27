using System;
using System.Collections.Generic;

namespace TecniHub.Domain.Entities
{
    public class Especialidad
    {
        public short Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        // Columnas base de auditoria
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        // Propiedad de navegación (Relación N:M con Técnicos a través de TecnicoEspecialidad)
        public ICollection<TecnicoEspecialidad> TecnicoEspecialidades { get; set; }

        public Especialidad()
        {
            TecnicoEspecialidades = new List<TecnicoEspecialidad>();
        }
    }
}