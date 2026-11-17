using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TecniHub.Domain.Entities;

namespace TecniHub.Infrastructure.Persistence.Configurations;

public sealed class ServicioSprint3Configuration : IEntityTypeConfiguration<ServicioSprint3>
{
    public void Configure(EntityTypeBuilder<ServicioSprint3> builder)
    {
        builder.ToTable("servicios", "core");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedOnAdd();
        builder.Property(x => x.CodigoFolio).HasColumnName("codigo_folio").HasMaxLength(25).IsRequired();
        builder.HasIndex(x => x.CodigoFolio).IsUnique();
        builder.Property(x => x.ClienteId).HasColumnName("cliente_id").HasColumnType("uuid");
        builder.Property(x => x.TecnicoId).HasColumnName("tecnico_id").HasColumnType("uuid");
        builder.Property(x => x.CategoriaId).HasColumnName("categoria_id").HasColumnType("smallint");
        builder.Property(x => x.TipoElectrodomestico).HasColumnName("tipo_electrodomestico").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Marca).HasColumnName("marca").HasMaxLength(100);
        builder.Property(x => x.Modelo).HasColumnName("modelo").HasMaxLength(100);
        builder.Property(x => x.DescripcionFalla).HasColumnName("descripcion_falla").IsRequired();
        builder.Property(x => x.UbicacionAproximada).HasColumnName("ubicacion_aproximada").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Municipio).HasColumnName("municipio").HasMaxLength(100);
        builder.Property(x => x.Latitud).HasColumnName("latitud").HasPrecision(10, 7);
        builder.Property(x => x.Longitud).HasColumnName("longitud").HasPrecision(10, 7);
        builder.Property(x => x.FechaSolicitud).HasColumnName("fecha_solicitud").HasColumnType("date");
        builder.Property(x => x.HorarioPreferido).HasColumnName("horario_preferido").HasMaxLength(100);
        builder.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(30).IsRequired();
        builder.Property(x => x.FechaAceptacion).HasColumnName("fecha_aceptacion");
        builder.Property(x => x.FechaSolicitudCierre).HasColumnName("fecha_solicitud_cierre");
        builder.Property(x => x.FechaCompletado).HasColumnName("fecha_completado");
        builder.Property(x => x.MotivoCancelacion).HasColumnName("motivo_cancelacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").ValueGeneratedOnAdd();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").ValueGeneratedOnAddOrUpdate();
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsActive).HasColumnName("is_active").ValueGeneratedOnAdd();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.DeletedBy).HasColumnName("deleted_by");
    }
}

public sealed class PiezaSprint3Configuration : IEntityTypeConfiguration<PiezaSprint3>
{
    public void Configure(EntityTypeBuilder<PiezaSprint3> builder)
    {
        builder.ToTable("piezas", "core");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedOnAdd();
        builder.Property(x => x.CodigoFolio).HasColumnName("codigo_folio").HasMaxLength(25).IsRequired();
        builder.HasIndex(x => x.CodigoFolio).IsUnique();
        builder.Property(x => x.TecnicoId).HasColumnName("tecnico_id").HasColumnType("uuid");
        builder.Property(x => x.CategoriaId).HasColumnName("categoria_id").HasColumnType("smallint");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").IsRequired();
        builder.Property(x => x.Condicion).HasColumnName("condicion").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Modalidad).HasColumnName("modalidad").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Precio).HasColumnName("precio").HasPrecision(10, 2);
        builder.Property(x => x.PiezaIntercambioDeseada).HasColumnName("pieza_intercambio_deseada");
        builder.Property(x => x.MedioContacto).HasColumnName("medio_contacto").HasMaxLength(255).IsRequired();
        builder.Property(x => x.UbicacionEntrega).HasColumnName("ubicacion_entrega").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(30).IsRequired();
        builder.Property(x => x.CompradorTecnicoId).HasColumnName("comprador_tecnico_id").HasColumnType("uuid");
        builder.Property(x => x.FechaCompletado).HasColumnName("fecha_completado");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").ValueGeneratedOnAdd();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").ValueGeneratedOnAddOrUpdate();
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsActive).HasColumnName("is_active").ValueGeneratedOnAdd();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.DeletedBy).HasColumnName("deleted_by");
    }
}

public sealed class CategoriaSprint3Configuration : IEntityTypeConfiguration<CategoriaSprint3>
{
    public void Configure(EntityTypeBuilder<CategoriaSprint3> builder)
    {
        builder.ToTable("categorias_electrodomesticos", "core");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("smallint");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion");
        builder.Property(x => x.IconoUrl).HasColumnName("icono_url").HasMaxLength(255);
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
    }
}

public sealed class ItinerarioSprint3Configuration : IEntityTypeConfiguration<ItinerarioSprint3>
{
    public void Configure(EntityTypeBuilder<ItinerarioSprint3> builder)
    {
        builder.ToTable("itinerario_tecnico", "core");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid");
        builder.Property(x => x.TecnicoId).HasColumnName("tecnico_id").HasColumnType("uuid");
        builder.Property(x => x.ServicioId).HasColumnName("servicio_id").HasColumnType("uuid");
        builder.Property(x => x.FechaProgramada).HasColumnName("fecha_programada").HasColumnType("date");
        builder.Property(x => x.HoraInicio).HasColumnName("hora_inicio").HasColumnType("time without time zone");
        builder.Property(x => x.HoraFinEstimada).HasColumnName("hora_fin_estimada").HasColumnType("time without time zone");
        builder.Property(x => x.NotasCita).HasColumnName("notas_cita");
        builder.Property(x => x.EstadoCita).HasColumnName("estado_cita").HasMaxLength(30);
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
    }
}
