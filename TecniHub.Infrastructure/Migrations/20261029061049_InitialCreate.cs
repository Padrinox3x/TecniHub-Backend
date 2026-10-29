using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TecniHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_ultimo_acceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RolId = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_RolId",
                        column: x => x.RolId,
                        principalSchema: "security",
                        principalTable: "roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "clientes",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    numero_telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    direccion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clientes_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "security",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "intentos_login",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    Exitoso = table.Column<bool>(type: "boolean", nullable: false),
                    IpOrigen = table.Column<string>(type: "text", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    MotivoFallo = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_intentos_login", x => x.Id);
                    table.ForeignKey(
                        name: "FK_intentos_login_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "security",
                        principalTable: "usuarios",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "sesiones",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    IpOrigen = table.Column<string>(type: "text", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    ExpiraEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevocadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesiones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sesiones_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "security",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clientes_UsuarioId",
                schema: "core",
                table: "clientes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_intentos_login_UsuarioId",
                schema: "audit",
                table: "intentos_login",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_UsuarioId",
                schema: "security",
                table: "sesiones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                schema: "security",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_RolId",
                schema: "security",
                table: "usuarios",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clientes",
                schema: "core");

            migrationBuilder.DropTable(
                name: "intentos_login",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "sesiones",
                schema: "security");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "security");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "security");
        }
    }
}
