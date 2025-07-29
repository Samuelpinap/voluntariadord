using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoluntariadoConectadoRD.Migrations
{
    /// <inheritdoc />
    public partial class AuthenticationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "usuarios",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "fechaCreacion",
                table: "usuarios",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "estatus",
                table: "usuarios",
                newName: "Estatus");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "usuarios",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "clave",
                table: "usuarios",
                newName: "PasswordHash");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Estatus",
                table: "usuarios",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "usuarios",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "usuarios",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "usuarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Rol",
                table: "usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "usuarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "organizaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SitioWeb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumeroRegistro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaVerificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_organizaciones_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organizaciones_Email",
                table: "organizaciones",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organizaciones_UsuarioId",
                table: "organizaciones",
                column: "UsuarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organizaciones");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_Email",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Apellido",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Rol",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "usuarios");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "usuarios",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "usuarios",
                newName: "fechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Estatus",
                table: "usuarios",
                newName: "estatus");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "usuarios",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "usuarios",
                newName: "clave");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                table: "usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "estatus",
                table: "usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
