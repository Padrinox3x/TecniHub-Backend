using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TecniHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTablonServiciosPiezas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "publicaciones_servicio",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tipo_electrodomestico = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion_falla = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ubicacion_aproximada = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    imagen_referencia = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TecnicoId = table.Column<int>(type: "integer", nullable: true),
                    fecha_publicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_publicaciones_servicio", x => x.id);
                    table.ForeignKey(
                        name: "FK_publicaciones_servicio_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "core",
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_publicaciones_servicio_tecnicos_TecnicoId",
                        column: x => x.TecnicoId,
                        principalSchema: "core",
                        principalTable: "tecnicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "publicaciones_pieza",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TecnicoId = table.Column<int>(type: "integer", nullable: false),
                    nombre_pieza = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    categoria_aparato = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    condicion_pieza = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tipo_transaccion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descripcion_detalle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    precio = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    datos_contacto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    fotografia = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado_disponibilidad = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_publicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_publicaciones_pieza", x => x.id);
                    table.ForeignKey(
                        name: "FK_publicaciones_pieza_tecnicos_TecnicoId",
                        column: x => x.TecnicoId,
                        principalSchema: "core",
                        principalTable: "tecnicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_publicaciones_servicio_ClienteId",
                schema: "core",
                table: "publicaciones_servicio",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_publicaciones_servicio_TecnicoId",
                schema: "core",
                table: "publicaciones_servicio",
                column: "TecnicoId");

            migrationBuilder.CreateIndex(
                name: "IX_publicaciones_pieza_TecnicoId",
                schema: "core",
                table: "publicaciones_pieza",
                column: "TecnicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "publicaciones_servicio",
                schema: "core");

            migrationBuilder.DropTable(
                name: "publicaciones_pieza",
                schema: "core");
        }
    }
}
