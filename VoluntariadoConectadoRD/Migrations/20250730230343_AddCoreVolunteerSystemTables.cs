using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoluntariadoConectadoRD.Migrations
{
    /// <inheritdoc />
    public partial class AddCoreVolunteerSystemTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add categoria eventos table
            migrationBuilder.CreateTable(
                name: "TM_categorias_evento",
                columns: table => new
                {
                    id_categoria = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_categorias_evento", x => x.id_categoria);
                });

            // Add voluntarios table
            migrationBuilder.CreateTable(
                name: "TM_voluntarios",
                columns: table => new
                {
                    id_voluntario = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cuenta = table.Column<int>(type: "int", nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    disponibilidad = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, defaultValue: "disponible"),
                    anos_experiencia = table.Column<int>(type: "int", nullable: true),
                    habilidades = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    horas_voluntariado = table.Column<short>(type: "smallint", nullable: false),
                    pais = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    flag_activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_voluntarios", x => x.id_voluntario);
                    table.ForeignKey(
                        name: "FK_TM_voluntarios_usuarios_id_cuenta",
                        column: x => x.id_cuenta,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                    table.CheckConstraint("CK_TM_voluntarios_disponibilidad", 
                        "[disponibilidad] IN ('flexible', 'estricto', 'disponible', 'no disponible')");
                    table.CheckConstraint("CK_TM_voluntarios_habilidades", 
                        "[habilidades] IN ('manual', 'oratoria', 'medicina', 'donante', 'otro')");
                });

            // Add administradores table
            migrationBuilder.CreateTable(
                name: "TM_administradores",
                columns: table => new
                {
                    id_administrador = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cuenta = table.Column<int>(type: "int", nullable: true),
                    flag_activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_administradores", x => x.id_administrador);
                    table.ForeignKey(
                        name: "FK_TM_administradores_usuarios_id_cuenta",
                        column: x => x.id_cuenta,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                });

            // Add eventos table
            migrationBuilder.CreateTable(
                name: "TM_eventos",
                columns: table => new
                {
                    id_evento = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    titulo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    ubicacion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    fecha = table.Column<DateTime>(type: "date", nullable: true),
                    hora_inicio = table.Column<TimeSpan>(type: "time", nullable: true),
                    hora_finalizacion = table.Column<TimeSpan>(type: "time", nullable: true),
                    id_organizacion = table.Column<int>(type: "int", nullable: true),
                    id_categoria = table.Column<long>(type: "bigint", nullable: true),
                    estado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "pautado"),
                    flag_activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_eventos", x => x.id_evento);
                    table.ForeignKey(
                        name: "FK_TM_eventos_organizaciones_id_organizacion",
                        column: x => x.id_organizacion,
                        principalTable: "organizaciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TM_eventos_TM_categorias_evento_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "TM_categorias_evento",
                        principalColumn: "id_categoria");
                    table.CheckConstraint("CK_TM_eventos_estado", 
                        "[estado] IN ('pautado', 'en proceso', 'completado', 'cancelado', 'pendiente')");
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_TM_voluntarios_id_cuenta",
                table: "TM_voluntarios",
                column: "id_cuenta",
                unique: true,
                filter: "[id_cuenta] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TM_administradores_id_cuenta",
                table: "TM_administradores",
                column: "id_cuenta",
                unique: true,
                filter: "[id_cuenta] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TM_eventos_id_organizacion",
                table: "TM_eventos",
                column: "id_organizacion");

            migrationBuilder.CreateIndex(
                name: "IX_TM_eventos_id_categoria",
                table: "TM_eventos",
                column: "id_categoria");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TM_eventos");
            migrationBuilder.DropTable(name: "TM_administradores");
            migrationBuilder.DropTable(name: "TM_voluntarios");
            migrationBuilder.DropTable(name: "TM_categorias_evento");
        }
    }
}
