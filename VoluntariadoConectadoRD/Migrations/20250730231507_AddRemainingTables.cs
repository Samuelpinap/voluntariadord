using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoluntariadoConectadoRD.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add TM_cuentas table
            migrationBuilder.CreateTable(
                name: "TM_cuentas",
                columns: table => new
                {
                    id_cuenta = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    correo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    telefono = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    contrasena_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    contrasena_salt = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    total_resenas = table.Column<short>(type: "smallint", nullable: false, defaultValue: 0),
                    foto_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    flag_bloqueada = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    flag_notificaciones = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    flag_activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_cuentas", x => x.id_cuenta);
                });

            // Add TD_inscripciones_voluntario_evento
            migrationBuilder.CreateTable(
                name: "TD_inscripciones_voluntario_evento",
                columns: table => new
                {
                    id_inscripcion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_voluntario = table.Column<long>(type: "bigint", nullable: false),
                    id_evento = table.Column<long>(type: "bigint", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "inscrito"),
                    fecha_inscripcion = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "GETDATE()"),
                    fecha_participacion = table.Column<DateTime>(type: "date", nullable: false),
                    comentarios = table.Column<string>(type: "text", nullable: true),
                    flag_activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TD_inscripciones_voluntario_evento", x => x.id_inscripcion);
                    table.ForeignKey(
                        name: "FK_TD_inscripciones_voluntario_evento_TM_voluntarios_id_voluntario",
                        column: x => x.id_voluntario,
                        principalTable: "TM_voluntarios",
                        principalColumn: "id_voluntario");
                    table.ForeignKey(
                        name: "FK_TD_inscripciones_voluntario_evento_TM_eventos_id_evento",
                        column: x => x.id_evento,
                        principalTable: "TM_eventos",
                        principalColumn: "id_evento");
                    table.CheckConstraint("CK_TD_inscripciones_voluntario_evento_estado", 
                        "[estado] IN ('inscrito', 'pendiente', 'cancelado')");
                });

            // Add TM_resenas
            migrationBuilder.CreateTable(
                name: "TM_resenas",
                columns: table => new
                {
                    id_resena = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cuenta_autora = table.Column<long>(type: "bigint", nullable: false),
                    id_organizacion_recipiente = table.Column<long>(type: "bigint", nullable: false),
                    texto = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    calificacion = table.Column<int>(type: "int", nullable: true),
                    fecha = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "GETDATE()"),
                    flag_activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_resenas", x => x.id_resena);
                    table.ForeignKey(
                        name: "FK_TM_resenas_TM_cuentas_id_cuenta_autora",
                        column: x => x.id_cuenta_autora,
                        principalTable: "TM_cuentas",
                        principalColumn: "id_cuenta");
                });

            // Add TM_mensajes
            migrationBuilder.CreateTable(
                name: "TM_mensajes",
                columns: table => new
                {
                    id_mensaje = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_remitente = table.Column<long>(type: "bigint", nullable: false),
                    id_destinatario = table.Column<long>(type: "bigint", nullable: false),
                    contenido = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    flag_leido = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    flag_activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_mensajes", x => x.id_mensaje);
                    table.ForeignKey(
                        name: "FK_TM_mensajes_TM_cuentas_id_remitente",
                        column: x => x.id_remitente,
                        principalTable: "TM_cuentas",
                        principalColumn: "id_cuenta");
                    table.ForeignKey(
                        name: "FK_TM_mensajes_TM_cuentas_id_destinatario",
                        column: x => x.id_destinatario,
                        principalTable: "TM_cuentas",
                        principalColumn: "id_cuenta");
                });

            // Add TM_notificaciones
            migrationBuilder.CreateTable(
                name: "TM_notificaciones",
                columns: table => new
                {
                    id_notificacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cuenta = table.Column<long>(type: "bigint", nullable: false),
                    id_mensaje = table.Column<long>(type: "bigint", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    mensaje = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    fecha = table.Column<DateTime>(type: "smalldatetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_notificaciones", x => x.id_notificacion);
                    table.ForeignKey(
                        name: "FK_TM_notificaciones_TM_cuentas_id_cuenta",
                        column: x => x.id_cuenta,
                        principalTable: "TM_cuentas",
                        principalColumn: "id_cuenta");
                    table.ForeignKey(
                        name: "FK_TM_notificaciones_TM_mensajes_id_mensaje",
                        column: x => x.id_mensaje,
                        principalTable: "TM_mensajes",
                        principalColumn: "id_mensaje");
                    table.CheckConstraint("CK_TM_notificaciones_tipo", 
                        "[tipo] IN ('sistema', 'mensaje', 'otro')");
                });

            // Add TM_documentos_legales
            migrationBuilder.CreateTable(
                name: "TM_documentos_legales",
                columns: table => new
                {
                    id_documento = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_organizacion = table.Column<int>(type: "int", nullable: false),
                    tipo_documento = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    archivo_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    fecha_subida = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    estado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, defaultValue: "pendiente"),
                    flag_activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_documentos_legales", x => x.id_documento);
                    table.ForeignKey(
                        name: "FK_TM_documentos_legales_organizaciones_id_organizacion",
                        column: x => x.id_organizacion,
                        principalTable: "organizaciones",
                        principalColumn: "Id");
                    table.CheckConstraint("CK_TM_documentos_legales_estado", 
                        "[estado] IN ('pendiente', 'revisado', 'aprobado', 'rechazado', 'cancelado', 'vigente', 'vencido')");
                });

            // Add TM_logros
            migrationBuilder.CreateTable(
                name: "TM_logros",
                columns: table => new
                {
                    id_logro = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    titulo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    foto_logro = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    flag_activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_logros", x => x.id_logro);
                });

            // Add TD_logros_cuenta
            migrationBuilder.CreateTable(
                name: "TD_logros_cuenta",
                columns: table => new
                {
                    id_conexion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cuenta = table.Column<long>(type: "bigint", nullable: false),
                    id_logro = table.Column<long>(type: "bigint", nullable: false),
                    fecha_otorgado = table.Column<DateTime>(type: "date", nullable: false),
                    observaciones = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TD_logros_cuenta", x => x.id_conexion);
                    table.ForeignKey(
                        name: "FK_TD_logros_cuenta_TM_cuentas_id_cuenta",
                        column: x => x.id_cuenta,
                        principalTable: "TM_cuentas",
                        principalColumn: "id_cuenta");
                    table.ForeignKey(
                        name: "FK_TD_logros_cuenta_TM_logros_id_logro",
                        column: x => x.id_logro,
                        principalTable: "TM_logros",
                        principalColumn: "id_logro");
                });

            // Add TM_donaciones
            migrationBuilder.CreateTable(
                name: "TM_donaciones",
                columns: table => new
                {
                    id_donacion = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cuenta_donante = table.Column<long>(type: "bigint", nullable: true),
                    id_organizacion_recipiente = table.Column<int>(type: "int", nullable: true),
                    monto = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    recibo_generado = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    mensaje = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_donaciones", x => x.id_donacion);
                    table.ForeignKey(
                        name: "FK_TM_donaciones_TM_cuentas_id_cuenta_donante",
                        column: x => x.id_cuenta_donante,
                        principalTable: "TM_cuentas",
                        principalColumn: "id_cuenta");
                    table.ForeignKey(
                        name: "FK_TM_donaciones_organizaciones_id_organizacion_recipiente",
                        column: x => x.id_organizacion_recipiente,
                        principalTable: "organizaciones",
                        principalColumn: "Id");
                });

            // Add TA_auditoria_administrativa
            migrationBuilder.CreateTable(
                name: "TA_auditoria_administrativa",
                columns: table => new
                {
                    id_auditoria = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tiempo_modificado = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    modificado_por = table.Column<long>(type: "bigint", nullable: false),
                    tipo_accion = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    tabla_afectada = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    id_fila_afectada = table.Column<long>(type: "bigint", nullable: false),
                    data_vieja = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    data_nueva = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TA_auditoria_administrativa", x => x.id_auditoria);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_TM_cuentas_correo",
                table: "TM_cuentas",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TD_inscripciones_voluntario_evento_id_voluntario",
                table: "TD_inscripciones_voluntario_evento",
                column: "id_voluntario");

            migrationBuilder.CreateIndex(
                name: "IX_TD_inscripciones_voluntario_evento_id_evento",
                table: "TD_inscripciones_voluntario_evento",
                column: "id_evento");

            migrationBuilder.CreateIndex(
                name: "IX_TM_resenas_id_cuenta_autora",
                table: "TM_resenas",
                column: "id_cuenta_autora");

            migrationBuilder.CreateIndex(
                name: "IX_TM_mensajes_id_remitente",
                table: "TM_mensajes",
                column: "id_remitente");

            migrationBuilder.CreateIndex(
                name: "IX_TM_mensajes_id_destinatario",
                table: "TM_mensajes",
                column: "id_destinatario");

            migrationBuilder.CreateIndex(
                name: "IX_TM_notificaciones_id_cuenta",
                table: "TM_notificaciones",
                column: "id_cuenta");

            migrationBuilder.CreateIndex(
                name: "IX_TM_notificaciones_id_mensaje",
                table: "TM_notificaciones",
                column: "id_mensaje");

            migrationBuilder.CreateIndex(
                name: "IX_TM_documentos_legales_id_organizacion",
                table: "TM_documentos_legales",
                column: "id_organizacion");

            migrationBuilder.CreateIndex(
                name: "IX_TD_logros_cuenta_id_cuenta",
                table: "TD_logros_cuenta",
                column: "id_cuenta");

            migrationBuilder.CreateIndex(
                name: "IX_TD_logros_cuenta_id_logro",
                table: "TD_logros_cuenta",
                column: "id_logro");

            migrationBuilder.CreateIndex(
                name: "IX_TM_donaciones_id_cuenta_donante",
                table: "TM_donaciones",
                column: "id_cuenta_donante");

            migrationBuilder.CreateIndex(
                name: "IX_TM_donaciones_id_organizacion_recipiente",
                table: "TM_donaciones",
                column: "id_organizacion_recipiente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TA_auditoria_administrativa");
            migrationBuilder.DropTable(name: "TM_donaciones");
            migrationBuilder.DropTable(name: "TD_logros_cuenta");
            migrationBuilder.DropTable(name: "TM_logros");
            migrationBuilder.DropTable(name: "TM_documentos_legales");
            migrationBuilder.DropTable(name: "TM_notificaciones");
            migrationBuilder.DropTable(name: "TM_mensajes");
            migrationBuilder.DropTable(name: "TM_resenas");
            migrationBuilder.DropTable(name: "TD_inscripciones_voluntario_evento");
            migrationBuilder.DropTable(name: "TM_cuentas");
        }
    }
}
