using System;

namespace TecniHub.Domain.Entities
{
    public class PiezaImagen
    {
        public Guid Id { get; set; }
        public Guid PiezaId { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool EsPrincipal { get; set; }

        // Columnas base de auditoria
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        // Propiedad de navegación (Relación N:1 con la publicación de la pieza)
        public PublicacionPieza Pieza { get; set; } = null!;
    }
}