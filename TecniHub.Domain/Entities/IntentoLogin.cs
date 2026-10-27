using System;

namespace TecniHub.Domain.Entities
{
    public class IntentoLogin
    {
        public long Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public Guid? UsuarioId { get; set; }
        public bool Exitoso { get; set; }
        public string IpOrigen { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? MotivoFallo { get; set; }
        public DateTime CreatedAt { get; set; }

        // Propiedad de navegación opcional (Relación N:1 con Usuario)
        public Usuario? Usuario { get; set; }
    }
}