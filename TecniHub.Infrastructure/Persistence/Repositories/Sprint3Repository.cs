using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NpgsqlTypes;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.DTOs.Sprint3;
using TecniHub.Domain.Entities;

namespace TecniHub.Infrastructure.Persistence.Repositories;

public sealed class Sprint3Repository(TecniHubDbContext context) : ISprint3Repository
{
    public async Task<ServicioCreadoDto> CrearServicioAsync(CrearServicioRequestDto request, CancellationToken cancellationToken)
    {
        var fecha = request.FechaSolicitud ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var servicio = new ServicioSprint3
        {
            CodigoFolio = $"SRV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..21],
            ClienteId = request.ClienteId,
            CategoriaId = request.CategoriaId,
            TipoElectrodomestico = request.TipoElectrodomestico.Trim(),
            Marca = request.Marca?.Trim(),
            Modelo = request.Modelo?.Trim(),
            DescripcionFalla = request.DescripcionFalla.Trim(),
            UbicacionAproximada = request.UbicacionAproximada.Trim(),
            Municipio = request.Municipio?.Trim(),
            Latitud = request.Latitud,
            Longitud = request.Longitud,
            FechaSolicitud = fecha,
            HorarioPreferido = request.HorarioPreferido?.Trim(),
            Estado = "ABIERTO",
            CreatedBy = request.ClienteId,
            UpdatedBy = request.ClienteId
        };

        context.ServiciosSprint3.Add(servicio);
        await context.SaveChangesAsync(cancellationToken);
        return new ServicioCreadoDto(servicio.Id, servicio.CodigoFolio, servicio.Estado);
    }

    public async Task<PiezaPublicadaDto> PublicarPiezaAsync(PublicarPiezaRequestDto request, string? ipOrigen, CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere) await connection.OpenAsync(cancellationToken);
        try
        {
            var activeTransaction = context.Database.CurrentTransaction?.GetDbTransaction() as NpgsqlTransaction;
            await using var command = new NpgsqlCommand(
                "CALL core.sp_publicar_pieza(@tecnico, @categoria, @nombre, @descripcion, @condicion, @modalidad, @precio, @pieza, @contacto, @ubicacion, @actor, @ip, NULL)", connection, activeTransaction);
            command.Parameters.AddWithValue("tecnico", NpgsqlDbType.Uuid, request.TecnicoId);
            command.Parameters.AddWithValue("categoria", NpgsqlDbType.Smallint, request.CategoriaId);
            command.Parameters.AddWithValue("nombre", NpgsqlDbType.Varchar, request.Nombre.Trim());
            command.Parameters.AddWithValue("descripcion", NpgsqlDbType.Text, request.Descripcion.Trim());
            command.Parameters.AddWithValue("condicion", NpgsqlDbType.Varchar, request.Condicion);
            command.Parameters.AddWithValue("modalidad", NpgsqlDbType.Varchar, request.Modalidad);
            command.Parameters.Add(new NpgsqlParameter("precio", NpgsqlDbType.Numeric) { Value = (object?)request.Precio ?? DBNull.Value });
            command.Parameters.Add(new NpgsqlParameter("pieza", NpgsqlDbType.Text) { Value = (object?)request.PiezaIntercambioDeseada ?? DBNull.Value });
            command.Parameters.AddWithValue("contacto", NpgsqlDbType.Varchar, request.MedioContacto.Trim());
            command.Parameters.AddWithValue("ubicacion", NpgsqlDbType.Varchar, request.UbicacionEntrega.Trim());
            command.Parameters.AddWithValue("actor", NpgsqlDbType.Uuid, request.TecnicoId);
            command.Parameters.AddWithValue("ip", NpgsqlDbType.Varchar, ipOrigen ?? "unknown");
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException("La base no devolvió el identificador de la pieza.");
            return new PiezaPublicadaDto(reader.GetGuid(0));
        }
        finally
        {
            if (openedHere) await connection.CloseAsync();
        }
    }

    public async Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken) =>
        await context.CategoriasSprint3.AsNoTracking()
            .Where(x => x.IsActive && x.DeletedAt == null)
            .OrderBy(x => x.Nombre)
            .Select(x => new CategoriaDto(x.Id, x.Nombre, x.Descripcion, x.IconoUrl))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PiezaDto>> BuscarPiezasAsync(BuscarPiezasRequestDto request, CancellationToken cancellationToken)
    {
        var query = context.PiezasSprint3.AsNoTracking()
            .Where(x => x.Estado == "DISPONIBLE" && x.IsActive && x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(request.Texto))
        {
            var term = $"%{request.Texto.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Nombre, term) || EF.Functions.ILike(x.Descripcion, term));
        }
        if (request.CategoriaId.HasValue) query = query.Where(x => x.CategoriaId == request.CategoriaId.Value);
        if (!string.IsNullOrWhiteSpace(request.Condicion)) query = query.Where(x => x.Condicion == request.Condicion.Trim().ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(request.Modalidad)) query = query.Where(x => x.Modalidad == request.Modalidad.Trim().ToUpperInvariant());
        if (request.PrecioMaximo.HasValue) query = query.Where(x => x.Precio <= request.PrecioMaximo.Value);

        return await query.OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .Select(x => new PiezaDto(x.Id, x.CodigoFolio, x.CategoriaId, x.Nombre, x.Descripcion,
                x.Condicion, x.Modalidad, x.Precio, x.PiezaIntercambioDeseada, x.MedioContacto, x.UbicacionEntrega))
            .ToListAsync(cancellationToken);
    }

    public async Task ProgramarItinerarioAsync(ProgramarItinerarioRequestDto request, string? ipOrigen, CancellationToken cancellationToken)
    {
        var existingTransaction = context.Database.CurrentTransaction;
        await using var ownedTransaction = existingTransaction is null
            ? await context.Database.BeginTransactionAsync(cancellationToken)
            : null;
        var transaction = existingTransaction ?? ownedTransaction!;
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        var dbTransaction = (NpgsqlTransaction)transaction.GetDbTransaction();

        await using (var lockCommand = new NpgsqlCommand(
            "SELECT pg_advisory_xact_lock(hashtextextended(CAST(@tecnico AS text), 0))", connection, dbTransaction))
        {
            lockCommand.Parameters.AddWithValue("tecnico", NpgsqlDbType.Uuid, request.TecnicoId);
            await lockCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var overlapCommand = new NpgsqlCommand("""
            SELECT EXISTS (
                SELECT 1 FROM core.itinerario_tecnico
                WHERE tecnico_id = @tecnico
                  AND fecha_programada = @fecha
                  AND is_active = TRUE
                  AND deleted_at IS NULL
                  AND estado_cita NOT IN ('CANCELADO', 'FINALIZADO')
                  AND servicio_id <> @servicio
                  AND @inicio < hora_fin_estimada
                  AND @fin > hora_inicio
            )
            """, connection, dbTransaction))
        {
            overlapCommand.Parameters.AddWithValue("tecnico", NpgsqlDbType.Uuid, request.TecnicoId);
            overlapCommand.Parameters.AddWithValue("fecha", NpgsqlDbType.Date, request.Fecha!.Value);
            overlapCommand.Parameters.AddWithValue("servicio", NpgsqlDbType.Uuid, request.ServicioId);
            overlapCommand.Parameters.AddWithValue("inicio", NpgsqlDbType.Time, request.HoraInicio!.Value);
            overlapCommand.Parameters.AddWithValue("fin", NpgsqlDbType.Time, request.HoraFinEstimada!.Value);
            if ((bool)(await overlapCommand.ExecuteScalarAsync(cancellationToken) ?? false))
                throw new InvalidOperationException("El técnico ya tiene un servicio programado que se traslapa con ese horario.");
        }

        await using (var scheduleCommand = new NpgsqlCommand(
            "CALL core.sp_programar_itinerario(@servicio, @tecnico, @fecha, @inicio, @fin, @notas, @actor, @ip)",
            connection, dbTransaction))
        {
            scheduleCommand.Parameters.AddWithValue("servicio", NpgsqlDbType.Uuid, request.ServicioId);
            scheduleCommand.Parameters.AddWithValue("tecnico", NpgsqlDbType.Uuid, request.TecnicoId);
            scheduleCommand.Parameters.AddWithValue("fecha", NpgsqlDbType.Date, request.Fecha!.Value);
            scheduleCommand.Parameters.AddWithValue("inicio", NpgsqlDbType.Time, request.HoraInicio!.Value);
            scheduleCommand.Parameters.AddWithValue("fin", NpgsqlDbType.Time, request.HoraFinEstimada!.Value);
            scheduleCommand.Parameters.Add(new NpgsqlParameter("notas", NpgsqlDbType.Text) { Value = (object?)request.Notas ?? DBNull.Value });
            scheduleCommand.Parameters.AddWithValue("actor", NpgsqlDbType.Uuid, request.ActorId);
            scheduleCommand.Parameters.AddWithValue("ip", NpgsqlDbType.Varchar, ipOrigen ?? "unknown");
            await scheduleCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        if (ownedTransaction is not null)
            await ownedTransaction.CommitAsync(cancellationToken);
    }
}
