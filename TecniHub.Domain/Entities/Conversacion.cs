using System;
using System.Collections.Generic;

namespace TecniHub.Domain.Entities
{
    public class Conversacion
    {
        public Guid Id { get; set; }
        public string TipoContexto { get; set; } = string.Empty; // 'SERVICIO' o 'PIEZA'
        public Guid? ServicioId { get; set; }
        public Guid? PiezaId { get; set; }
        public Guid UsuarioIniciadorId { get; set; }
        public Guid UsuarioReceptorId { get; set; }
        public string Estado { get; set; } = "ACTIVO";

        // Columnas base de auditoria
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        // Propiedades de navegación
        public PublicacionServicio? Servicio { get; set; }
        public PublicacionPieza? Pieza { get; set; }
        public Usuario UsuarioIniciador { get; set; } = null!;
        public Usuario UsuarioReceptor { get; set; } = null!;
        public ICollection<Mensaje> Mensajes { get; set; }

        public Conversacion()
        {
            Mensajes = new List<Mensaje>();
        }
    }
}