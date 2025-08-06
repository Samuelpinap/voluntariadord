using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoluntariadoConectadoRD.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IconoUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    RequisitoValor = table.Column<int>(type: "INTEGER", nullable: false),
                    EsActivo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "platform_stats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoluntariosActivos = table.Column<int>(type: "INTEGER", nullable: false),
                    OrganizacionesActivas = table.Column<int>(type: "INTEGER", nullable: false),
                    ProyectosActivos = table.Column<int>(type: "INTEGER", nullable: false),
                    HorasTotalesDonadas = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonasBeneficiadas = table.Column<int>(type: "INTEGER", nullable: false),
                    FondosRecaudados = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NotasEstadisticas = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_stats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    descripcion = table.Column<int>(type: "INTEGER", nullable: false),
                    estatus = table.Column<bool>(type: "INTEGER", nullable: false),
                    fechaCreacion = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EsActivo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    FechaNacimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Rol = table.Column<int>(type: "INTEGER", nullable: false),
                    Estatus = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Biografia = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Habilidades = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ExperienciaAnios = table.Column<int>(type: "INTEGER", nullable: true),
                    Disponibilidad = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HorasVoluntariado = table.Column<int>(type: "INTEGER", nullable: false),
                    CalificacionPromedio = table.Column<decimal>(type: "TEXT", precision: 3, scale: 2, nullable: false),
                    TotalResenas = table.Column<int>(type: "INTEGER", nullable: false),
                    PerfilCompleto = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organizaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    SitioWeb = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    NumeroRegistro = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Estatus = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaVerificacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TipoOrganizacion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FechaFundacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Mision = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Vision = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    AreasInteres = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PerfilCompleto = table.Column<bool>(type: "INTEGER", nullable: false),
                    Verificada = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "usuario_badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    BadgeId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaObtenido = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NotasObtencion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
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

            migrationBuilder.CreateTable(
                name: "usuario_skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    SkillId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nivel = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuario_skills_skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_skills_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "financial_reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Año = table.Column<int>(type: "INTEGER", nullable: false),
                    Trimestre = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalIngresos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalGastos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Resumen = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DocumentoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EsPublico = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_financial_reports_organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_resenas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioResenadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioAutorId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Calificacion = table.Column<int>(type: "INTEGER", nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "volunteer_opportunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Ubicacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DuracionHoras = table.Column<int>(type: "INTEGER", nullable: false),
                    VoluntariosRequeridos = table.Column<int>(type: "INTEGER", nullable: false),
                    VoluntariosInscritos = table.Column<int>(type: "INTEGER", nullable: false),
                    AreaInteres = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NivelExperiencia = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Requisitos = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Beneficios = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Estatus = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: false)
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
                name: "donations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FinancialReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    Donante = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Proposito = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DocumentoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EsRecurrente = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_donations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_donations_financial_reports_FinancialReportId",
                        column: x => x.FinancialReportId,
                        principalTable: "financial_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FinancialReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Justificacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DocumentoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_expenses_financial_reports_FinancialReportId",
                        column: x => x.FinancialReportId,
                        principalTable: "financial_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "volunteer_activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    OpportunityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaCompletada = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HorasCompletadas = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CalificacionVoluntario = table.Column<int>(type: "INTEGER", nullable: true),
                    CalificacionOrganizacion = table.Column<int>(type: "INTEGER", nullable: true),
                    ComentarioVoluntario = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ComentarioOrganizacion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_volunteer_activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_volunteer_activities_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_volunteer_activities_volunteer_opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "volunteer_opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "volunteer_applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    OpportunityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Mensaje = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Estatus = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaAplicacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaRespuesta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NotasOrganizacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
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
                name: "IX_donations_FinancialReportId",
                table: "donations",
                column: "FinancialReportId");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_FinancialReportId",
                table: "expenses",
                column: "FinancialReportId");

            migrationBuilder.CreateIndex(
                name: "IX_financial_reports_OrganizacionId",
                table: "financial_reports",
                column: "OrganizacionId");

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

            migrationBuilder.CreateIndex(
                name: "IX_usuario_skills_SkillId",
                table: "usuario_skills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_skills_UsuarioId_SkillId",
                table: "usuario_skills",
                columns: new[] { "UsuarioId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_volunteer_activities_OpportunityId",
                table: "volunteer_activities",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_volunteer_activities_UsuarioId",
                table: "volunteer_activities",
                column: "UsuarioId");

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
                name: "donations");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "platform_stats");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "usuario_badges");

            migrationBuilder.DropTable(
                name: "usuario_resenas");

            migrationBuilder.DropTable(
                name: "usuario_skills");

            migrationBuilder.DropTable(
                name: "volunteer_activities");

            migrationBuilder.DropTable(
                name: "volunteer_applications");

            migrationBuilder.DropTable(
                name: "financial_reports");

            migrationBuilder.DropTable(
                name: "badges");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "volunteer_opportunities");

            migrationBuilder.DropTable(
                name: "organizaciones");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
