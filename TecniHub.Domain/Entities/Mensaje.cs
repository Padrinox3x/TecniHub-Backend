using System;

namespace TecniHub.Domain.Entities
{
    public class Mensaje
    {
        public long Id { get; set; }
        public Guid ConversacionId { get; set; }
        public Guid EmisorId { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime EnviadoAt { get; set; }
        public DateTime? LeidoAt { get; set; }
        public bool IsActive { get; set; }

        // Propiedades de navegación
        public Conversacion Conversacion { get; set; } = null!;
        public Usuario Emisor { get; set; } = null!;
    }
}