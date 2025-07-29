using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoluntariadoConectadoRD.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteerOpportunities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "volunteer_opportunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DuracionHoras = table.Column<int>(type: "int", nullable: false),
                    VoluntariosRequeridos = table.Column<int>(type: "int", nullable: false),
                    VoluntariosInscritos = table.Column<int>(type: "int", nullable: false),
                    AreaInteres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NivelExperiencia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Requisitos = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Beneficios = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrganizacionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_volunteer_opportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_volunteer_opportunities_organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "volunteer_applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    OpportunityId = table.Column<int>(type: "int", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    FechaAplicacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRespuesta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotasOrganizacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_volunteer_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_volunteer_applications_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_volunteer_applications_volunteer_opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "volunteer_opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_volunteer_applications_OpportunityId",
                table: "volunteer_applications",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_volunteer_applications_UsuarioId_OpportunityId",
                table: "volunteer_applications",
                columns: new[] { "UsuarioId", "OpportunityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_volunteer_opportunities_OrganizacionId",
                table: "volunteer_opportunities",
                column: "OrganizacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "volunteer_applications");

            migrationBuilder.DropTable(
                name: "volunteer_opportunities");
        }
    }
}
