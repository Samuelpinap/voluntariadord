using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoluntariadoConectadoRD.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAndOrganizationProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "usuarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Biografia",
                table: "usuarios",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Disponibilidad",
                table: "usuarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperienciaPrevia",
                table: "usuarios",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Habilidades",
                table: "usuarios",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Intereses",
                table: "usuarios",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PerfilCompleto",
                table: "usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AreasEnfoque",
                table: "organizaciones",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CargoContacto",
                table: "organizaciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "organizaciones",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mision",
                table: "organizaciones",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PerfilCompleto",
                table: "organizaciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PersonaContacto",
                table: "organizaciones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoContacto",
                table: "organizaciones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vision",
                table: "organizaciones",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Biografia",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Disponibilidad",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "ExperienciaPrevia",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Habilidades",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Intereses",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "PerfilCompleto",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "AreasEnfoque",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "CargoContacto",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "Mision",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "PerfilCompleto",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "PersonaContacto",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "TelefonoContacto",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "Vision",
                table: "organizaciones");
        }
    }
}
