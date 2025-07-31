using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoluntariadoConectadoRD.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileAndReviewSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Biografia",
                table: "usuarios",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalificacionPromedio",
                table: "usuarios",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Disponibilidad",
                table: "usuarios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienciaAnios",
                table: "usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Habilidades",
                table: "usuarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HorasVoluntariado",
                table: "usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "usuarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalResenas",
                table: "usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IconoUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    RequisitoValor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuario_resenas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioResenadoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioAutorId = table.Column<int>(type: "int", nullable: false),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false),
                    Calificacion = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_resenas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuario_resenas_organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_resenas_usuarios_UsuarioAutorId",
                        column: x => x.UsuarioAutorId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_resenas_usuarios_UsuarioResenadoId",
                        column: x => x.UsuarioResenadoId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    BadgeId = table.Column<int>(type: "int", nullable: false),
                    FechaObtenido = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_badges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuario_badges_badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_badges_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuario_badges_BadgeId",
                table: "usuario_badges",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_badges_UsuarioId_BadgeId",
                table: "usuario_badges",
                columns: new[] { "UsuarioId", "BadgeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_resenas_OrganizacionId",
                table: "usuario_resenas",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_resenas_UsuarioAutorId",
                table: "usuario_resenas",
                column: "UsuarioAutorId");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_resenas_UsuarioResenadoId",
                table: "usuario_resenas",
                column: "UsuarioResenadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuario_badges");

            migrationBuilder.DropTable(
                name: "usuario_resenas");

            migrationBuilder.DropTable(
                name: "badges");

            migrationBuilder.DropColumn(
                name: "Biografia",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "CalificacionPromedio",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Disponibilidad",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "ExperienciaAnios",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Habilidades",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "HorasVoluntariado",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "TotalResenas",
                table: "usuarios");
        }
    }
}
