using System;
using System.Collections.Generic;

namespace TecniHub.Domain.Entities
{
    public class CategoriaElectrodomestico
    {
        public short Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? IconoUrl { get; set; }

        // Columnas base de auditoria
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        // Propiedades de navegación basadas en tu estructura actual
        public ICollection<PublicacionServicio> Servicios { get; set; }
        public ICollection<PublicacionPieza> Piezas { get; set; }

        public CategoriaElectrodomestico()
        {
            Servicios = new List<PublicacionServicio>();
            Piezas = new List<PublicacionPieza>();
        }
    }
}