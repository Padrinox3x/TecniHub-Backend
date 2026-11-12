using Microsoft.EntityFrameworkCore;
using TecniHub.Domain.Entities;
using TecniHub.Domain.ValueObjects;
using TecniHub.Infrastructure.Persistence.Configurations;

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
        public DbSet<Tecnico> Tecnicos => Set<Tecnico>();
        public DbSet<Sesion> Sesiones => Set<Sesion>();
        public DbSet<IntentoLogin> IntentosLogin => Set<IntentoLogin>();
        public DbSet<PublicacionServicio> PublicacionesServicio => Set<PublicacionServicio>();
        public DbSet<PublicacionPieza> PublicacionesPieza => Set<PublicacionPieza>();

        // Modelos de Sprint 3: se agregan sin cambiar los mapeos existentes de autenticación.
        public DbSet<ServicioSprint3> ServiciosSprint3 => Set<ServicioSprint3>();
        public DbSet<PiezaSprint3> PiezasSprint3 => Set<PiezaSprint3>();
        public DbSet<CategoriaSprint3> CategoriasSprint3 => Set<CategoriaSprint3>();
        public DbSet<ItinerarioSprint3> ItinerariosSprint3 => Set<ItinerarioSprint3>();

        // Modelos de Sprint 3: se agregan sin cambiar los mapeos existentes de autenticación.
        public DbSet<ServicioSprint3> ServiciosSprint3 => Set<ServicioSprint3>();
        public DbSet<PiezaSprint3> PiezasSprint3 => Set<PiezaSprint3>();
        public DbSet<CategoriaSprint3> CategoriasSprint3 => Set<CategoriaSprint3>();
        public DbSet<ItinerarioSprint3> ItinerariosSprint3 => Set<ItinerarioSprint3>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ServicioSprint3Configuration());
            modelBuilder.ApplyConfiguration(new PiezaSprint3Configuration());
            modelBuilder.ApplyConfiguration(new CategoriaSprint3Configuration());
            modelBuilder.ApplyConfiguration(new ItinerarioSprint3Configuration());

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

            modelBuilder.Entity<Tecnico>(builder =>
            {
                builder.ToTable("tecnicos", "core");
                builder.HasKey(t => t.Id);

                builder.Property(t => t.Id)
                       .HasColumnName("id");

                builder.Property(t => t.NombreCompleto)
                       .HasColumnName("nombre_completo")
                       .HasMaxLength(150)
                       .IsRequired();

                builder.Property(t => t.Correo)
                       .HasConversion(
                           c => c.Valor,
                           v => new CorreoElectronico(v)
                       )
                       .HasColumnName("email")
                       .HasMaxLength(150)
                       .IsRequired();

                builder.OwnsOne(t => t.NumeroTelefono, telBuilder =>
                {
                    telBuilder.Property(n => n.Valor)
                              .HasColumnName("numero_telefono")
                              .HasMaxLength(20)
                              .IsRequired();
                });

                builder.Property(t => t.Contrasena)
                       .HasColumnName("password_hash")
                       .IsRequired();

                builder.Property(t => t.FotoIdentificacion)
                       .HasColumnName("foto_identificacion")
                       .HasMaxLength(500);

                builder.Property(t => t.TieneNegocioFisico)
                       .HasColumnName("tiene_negocio_fisico")
                       .IsRequired();

                builder.OwnsOne(t => t.TelefonoNegocio, telBuilder =>
                {
                    telBuilder.Property(n => n.Valor)
                              .HasColumnName("telefono_negocio")
                              .HasMaxLength(20);
                });

                builder.Property(t => t.UbicacionEstablecimiento)
                       .HasColumnName("ubicacion_establecimiento")
                       .HasMaxLength(255);

                builder.Property(t => t.RedesSociales)
                       .HasColumnName("redes_sociales")
                       .HasMaxLength(500);

                builder.Property(t => t.CostoCotizacion)
                       .HasColumnName("costo_cotizacion")
                       .HasPrecision(10, 2);

                builder.Property(t => t.EvidenciaTrabajos)
                       .HasColumnName("evidencia_trabajos");

                builder.Property(t => t.Rol)
                       .HasColumnName("rol")
                       .HasMaxLength(20)
                       .IsRequired();

                builder.HasOne(t => t.Usuario)
                       .WithMany(u => u.Tecnicos)
                       .HasForeignKey(t => t.UsuarioId)
                       .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PublicacionServicio>(builder =>
            {
                builder.ToTable("publicaciones_servicio", "core");
                builder.HasKey(s => s.Id);

                builder.Property(s => s.Id)
                       .HasColumnName("id");

                builder.Property(s => s.Titulo)
                       .HasColumnName("titulo")
                       .HasMaxLength(150)
                       .IsRequired();

                builder.Property(s => s.TipoElectrodomestico)
                       .HasColumnName("tipo_electrodomestico")
                       .HasMaxLength(100)
                       .IsRequired();

                builder.Property(s => s.DescripcionFalla)
                       .HasColumnName("descripcion_falla")
                       .HasMaxLength(1000)
                       .IsRequired();

                builder.Property(s => s.UbicacionAproximada)
                       .HasColumnName("ubicacion_aproximada")
                       .HasMaxLength(255)
                       .IsRequired();

                builder.Property(s => s.ImagenReferencia)
                       .HasColumnName("imagen_referencia")
                       .HasMaxLength(500);

                builder.Property(s => s.Estado)
                       .HasColumnName("estado")
                       .HasMaxLength(30)
                       .IsRequired();

                builder.Property(s => s.FechaPublicacion)
                       .HasColumnName("fecha_publicacion")
                       .IsRequired();

                builder.HasOne(s => s.Cliente)
                       .WithMany()
                       .HasForeignKey(s => s.ClienteId)
                       .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(s => s.Tecnico)
                       .WithMany()
                       .HasForeignKey(s => s.TecnicoId)
                       .IsRequired(false)
                       .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<PublicacionPieza>(builder =>
            {
                builder.ToTable("publicaciones_pieza", "core");
                builder.HasKey(p => p.Id);

                builder.Property(p => p.Id)
                       .HasColumnName("id");

                builder.Property(p => p.NombrePieza)
                       .HasColumnName("nombre_pieza")
                       .HasMaxLength(150)
                       .IsRequired();

                builder.Property(p => p.CategoriaAparato)
                       .HasColumnName("categoria_aparato")
                       .HasMaxLength(100)
                       .IsRequired();

                builder.Property(p => p.CondicionPieza)
                       .HasColumnName("condicion_pieza")
                       .HasMaxLength(50)
                       .IsRequired();

                builder.Property(p => p.TipoTransaccion)
                       .HasColumnName("tipo_transaccion")
                       .HasMaxLength(30)
                       .IsRequired();

                builder.Property(p => p.DescripcionDetalle)
                       .HasColumnName("descripcion_detalle")
                       .HasMaxLength(1000);

                builder.Property(p => p.Precio)
                       .HasColumnName("precio")
                       .HasPrecision(10, 2);

                builder.Property(p => p.DatosContacto)
                       .HasColumnName("datos_contacto")
                       .HasMaxLength(255)
                       .IsRequired();

                builder.Property(p => p.Fotografia)
                       .HasColumnName("fotografia")
                       .HasMaxLength(500);

                builder.Property(p => p.EstadoDisponibilidad)
                       .HasColumnName("estado_disponibilidad")
                       .HasMaxLength(30)
                       .IsRequired();

                builder.Property(p => p.FechaPublicacion)
                       .HasColumnName("fecha_publicacion")
                       .IsRequired();

                builder.HasOne(p => p.Tecnico)
                       .WithMany()
                       .HasForeignKey(p => p.TecnicoId)
                       .OnDelete(DeleteBehavior.Cascade);
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
