using Microsoft.EntityFrameworkCore;
using TecniHub.Domain.Entities;
using TecniHub.Domain.ValueObjects;

namespace TecniHub.Infrastructure.Persistence
{
    public class TecniHubDbContext : DbContext
    {
        public TecniHubDbContext(DbContextOptions<TecniHubDbContext> options)
            : base(options)
        {
        }

        // DbSets principales para las historias de usuario de Sprint 1
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Sesion> Sesiones => Set<Sesion>();
        public DbSet<IntentoLogin> IntentosLogin => Set<IntentoLogin>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // ESQUEMA: security
            // ==========================================
            modelBuilder.Entity<Rol>(builder =>
            {
                builder.ToTable("roles", "security");
                builder.HasKey(r => r.Id);
                builder.Property(r => r.Codigo).HasMaxLength(20).IsRequired();
                builder.Property(r => r.Nombre).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<Usuario>(builder =>
            {
                builder.ToTable("usuarios", "security");

                builder.HasKey(u => u.Id);

                builder.Property(u => u.Id)
                       .HasColumnName("id");

                builder.Property(u => u.Correo)
                       .HasColumnName("email") // Asegúrate de que coincida con el nombre en tu script de BD en Neon
                       .HasMaxLength(150)
                       .IsRequired();

                builder.HasIndex(u => u.Correo)
                       .IsUnique();

                builder.Property(u => u.Contrasena)
                       .HasColumnName("password_hash")
                       .IsRequired();

                builder.Property(u => u.Rol)
                       .HasColumnName("rol")
                       .HasMaxLength(20)
                       .IsRequired();

                builder.Property(u => u.FechaUltimoAcceso)
                       .HasColumnName("fecha_ultimo_acceso");
            });
            modelBuilder.Entity<Sesion>(builder =>
            {
                builder.ToTable("sesiones", "security");
                builder.HasKey(s => s.Id);
                builder.Property(s => s.TokenHash).IsRequired();

                // Relación Sesion N:1 Usuario
                builder.HasOne(s => s.Usuario)
                       .WithMany()
                       .HasForeignKey(s => s.UsuarioId);
            });

            // ==========================================
            // ESQUEMA: core
            // ==========================================
            modelBuilder.Entity<Cliente>(builder =>
            {
                builder.ToTable("clientes", "core");
                builder.HasKey(c => c.Id);

                builder.Property(c => c.Nombre)
                       .HasColumnName("nombre")
                       .HasMaxLength(100)
                       .IsRequired();

                builder.Property(c => c.Apellidos) // En plural tal como está en tu dominio
                       .HasColumnName("apellidos")
                       .HasMaxLength(100)
                       .IsRequired();

                // Mapeo del Value Object CorreoElectronico a la columna de correo
                builder.Property(c => c.Correo)
                       .HasConversion(
                           c => c.Valor,                       // De Value Object a string para guardar en BD
                           v => new CorreoElectronico(v)        // De string en BD a Value Object al leer
                       )
                       .HasColumnName("email")
                       .HasMaxLength(150)
                       .IsRequired();

                // Mapeo del Value Object NumeroTelefono (si es propiedad compleja/poseída)
                builder.OwnsOne(c => c.NumeroTelefono, telBuilder =>
                {
                    telBuilder.Property(t => t.Valor) // Cambia 'Valor' por el nombre de la propiedad interna de tu ValueObject
                              .HasColumnName("numero_telefono")
                              .HasMaxLength(20);
                });

                builder.Property(c => c.Contrasena)
                       .HasColumnName("password_hash")
                       .IsRequired();

                builder.Property(c => c.Direccion)
                       .HasColumnName("direccion")
                       .HasMaxLength(255);

                builder.Property(c => c.Rol)
                       .HasColumnName("rol")
                       .HasMaxLength(20)
                       .IsRequired();
            });

            // ==========================================
            // ESQUEMA: audit
            // ==========================================
            modelBuilder.Entity<IntentoLogin>(builder =>
            {
                builder.ToTable("intentos_login", "audit");
                builder.HasKey(i => i.Id);
                builder.Property(i => i.Email).HasMaxLength(150).IsRequired();

                // Relación opcional con Usuario (si el usuario existía al intentar loguearse)
                builder.HasOne(i => i.Usuario)
                       .WithMany()
                       .HasForeignKey(i => i.UsuarioId)
                       .IsRequired(false);
            });
        }
    }
}