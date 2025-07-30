using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoluntariadoConectadoRD.Migrations
{
    /// <inheritdoc />
    public partial class AddUbicacionFieldToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ubicacion",
                table: "usuarios",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ubicacion",
                table: "usuarios");
        }
    }
}
