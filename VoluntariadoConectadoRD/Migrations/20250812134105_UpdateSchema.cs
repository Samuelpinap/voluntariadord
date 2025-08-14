using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoluntariadoConectadoRD.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VolunteerOpportunityId",
                table: "volunteer_applications",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "usuarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "usuarios",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion",
                table: "usuarios",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Certificacion",
                table: "usuario_skills",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAdquisicion",
                table: "usuario_skills",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazonOtorgamiento",
                table: "usuario_badges",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId1",
                table: "usuario_badges",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "skills",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IconoUrl",
                table: "skills",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsAutomatico",
                table: "badges",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Requisitos",
                table: "badges",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecipientId = table.Column<int>(type: "INTEGER", nullable: false),
                    SenderId = table.Column<int>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ActionUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_usuarios_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_usuarios_SenderId",
                        column: x => x.SenderId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserOnlineStatuses",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsOnline = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConnectionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOnlineStatuses", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserOnlineStatuses_usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    User1Id = table.Column<int>(type: "INTEGER", nullable: false),
                    User2Id = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastMessageId = table.Column<int>(type: "INTEGER", nullable: true),
                    User1HasUnread = table.Column<bool>(type: "INTEGER", nullable: false),
                    User2HasUnread = table.Column<bool>(type: "INTEGER", nullable: false),
                    User1LastSeen = table.Column<DateTime>(type: "TEXT", nullable: true),
                    User2LastSeen = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_usuarios_User1Id",
                        column: x => x.User1Id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_usuarios_User2Id",
                        column: x => x.User2Id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecipientId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReplyToMessageId = table.Column<int>(type: "INTEGER", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AttachmentFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    AttachmentMimeType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AttachmentSize = table.Column<long>(type: "INTEGER", nullable: true),
                    ConversationId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Messages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Messages_usuarios_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_usuarios_SenderId",
                        column: x => x.SenderId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_volunteer_applications_VolunteerOpportunityId",
                table: "volunteer_applications",
                column: "VolunteerOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_badges_UsuarioId1",
                table: "usuario_badges",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_IsArchived",
                table: "Conversations",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_LastMessageAt",
                table: "Conversations",
                column: "LastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_LastMessageId",
                table: "Conversations",
                column: "LastMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User1Id",
                table: "Conversations",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User1Id_User1HasUnread",
                table: "Conversations",
                columns: new[] { "User1Id", "User1HasUnread" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User2Id",
                table: "Conversations",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User2Id_User2HasUnread",
                table: "Conversations",
                columns: new[] { "User2Id", "User2HasUnread" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IsDeleted",
                table: "Messages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IsRead",
                table: "Messages",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RecipientId",
                table: "Messages",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RecipientId_IsRead_IsDeleted",
                table: "Messages",
                columns: new[] { "RecipientId", "IsRead", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReplyToMessageId",
                table: "Messages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SentAt",
                table: "Messages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SenderId",
                table: "Notifications",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                table: "Notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_UserOnlineStatuses_IsOnline",
                table: "UserOnlineStatuses",
                column: "IsOnline");

            migrationBuilder.CreateIndex(
                name: "IX_UserOnlineStatuses_LastSeen",
                table: "UserOnlineStatuses",
                column: "LastSeen");

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_badges_usuarios_UsuarioId1",
                table: "usuario_badges",
                column: "UsuarioId1",
                principalTable: "usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_volunteer_applications_volunteer_opportunities_VolunteerOpportunityId",
                table: "volunteer_applications",
                column: "VolunteerOpportunityId",
                principalTable: "volunteer_opportunities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Messages_LastMessageId",
                table: "Conversations",
                column: "LastMessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usuario_badges_usuarios_UsuarioId1",
                table: "usuario_badges");

            migrationBuilder.DropForeignKey(
                name: "FK_volunteer_applications_volunteer_opportunities_VolunteerOpportunityId",
                table: "volunteer_applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Messages_LastMessageId",
                table: "Conversations");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "UserOnlineStatuses");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_volunteer_applications_VolunteerOpportunityId",
                table: "volunteer_applications");

            migrationBuilder.DropIndex(
                name: "IX_usuario_badges_UsuarioId1",
                table: "usuario_badges");

            migrationBuilder.DropColumn(
                name: "VolunteerOpportunityId",
                table: "volunteer_applications");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Ubicacion",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Certificacion",
                table: "usuario_skills");

            migrationBuilder.DropColumn(
                name: "FechaAdquisicion",
                table: "usuario_skills");

            migrationBuilder.DropColumn(
                name: "RazonOtorgamiento",
                table: "usuario_badges");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "usuario_badges");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "skills");

            migrationBuilder.DropColumn(
                name: "IconoUrl",
                table: "skills");

            migrationBuilder.DropColumn(
                name: "EsAutomatico",
                table: "badges");

            migrationBuilder.DropColumn(
                name: "Requisitos",
                table: "badges");
        }
    }
}
