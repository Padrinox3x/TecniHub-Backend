using System;

namespace TecniHub.Domain.Entities
{
    public class Sesion
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public string IpOrigen { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public DateTime ExpiraEn { get; set; }
        public DateTime? RevocadoEn { get; set; }

        // Columnas base de tiempo y auditoria
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Propiedad de navegación (Relación N:1 con Usuario)
        public Usuario Usuario { get; set; } = null!;
    }
}