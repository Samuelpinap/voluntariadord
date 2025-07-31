using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoluntariadoConectadoRD.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileExtensionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaNacimiento",
                table: "usuarios",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "ExperienciaAnios",
                table: "usuarios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "PerfilCompleto",
                table: "usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AreasInteres",
                table: "organizaciones",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFundacion",
                table: "organizaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
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
                name: "TipoOrganizacion",
                table: "organizaciones",
                type: "nvarchar(100)",
                maxLength: 100,
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
                name: "PerfilCompleto",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "AreasInteres",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "FechaFundacion",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "Mision",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "PerfilCompleto",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "TipoOrganizacion",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "Vision",
                table: "organizaciones");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaNacimiento",
                table: "usuarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ExperienciaAnios",
                table: "usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
