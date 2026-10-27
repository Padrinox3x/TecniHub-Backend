using System;

namespace TecniHub.Domain.Entities
{
    public class RecuperacionPassword
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
        public DateTime? UtilizadoEn { get; set; }
        public string IpSolicitud { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Propiedad de navegación (Relación N:1 con Usuario)
        public Usuario Usuario { get; set; } = null!;
    }
}