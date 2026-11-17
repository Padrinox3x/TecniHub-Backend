using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TecniHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTecnicosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tecnicos",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    nombre_completo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    numero_telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    foto_identificacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tiene_negocio_fisico = table.Column<bool>(type: "boolean", nullable: false),
                    telefono_negocio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ubicacion_establecimiento = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    redes_sociales = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    costo_cotizacion = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    evidencia_trabajos = table.Column<string>(type: "text", nullable: true),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tecnicos", x => x.id);
                    table.ForeignKey(
                        name: "FK_tecnicos_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "security",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tecnicos_UsuarioId",
                schema: "core",
                table: "tecnicos",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tecnicos",
                schema: "core");
        }
    }
}
