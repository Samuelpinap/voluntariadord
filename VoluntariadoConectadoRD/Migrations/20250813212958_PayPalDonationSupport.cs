using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoluntariadoConectadoRD.Migrations
{
    /// <inheritdoc />
    public partial class PayPalDonationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SaldoActual",
                table: "organizaciones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "FinancialReportId",
                table: "donations",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "EstadoPago",
                table: "donations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MetodoPago",
                table: "donations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizacionId",
                table: "donations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayPalOrderId",
                table: "donations",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayPalPayerEmail",
                table: "donations",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayPalPayerId",
                table: "donations",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayPalPaymentStatus",
                table: "donations",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayPalTransactionId",
                table: "donations",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "paypal_transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PayPalOrderId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PayPalTransactionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    DonationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PayerEmail = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    PayerId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PayerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RawPayPalResponse = table.Column<string>(type: "TEXT", nullable: true),
                    WebhookData = table.Column<string>(type: "TEXT", nullable: true),
                    TransactionType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paypal_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_paypal_transactions_donations_DonationId",
                        column: x => x.DonationId,
                        principalTable: "donations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_paypal_transactions_organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_donations_OrganizacionId",
                table: "donations",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_donations_PayPalOrderId",
                table: "donations",
                column: "PayPalOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_donations_PayPalTransactionId",
                table: "donations",
                column: "PayPalTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_paypal_transactions_CreatedAt",
                table: "paypal_transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_paypal_transactions_DonationId",
                table: "paypal_transactions",
                column: "DonationId");

            migrationBuilder.CreateIndex(
                name: "IX_paypal_transactions_OrganizacionId",
                table: "paypal_transactions",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_paypal_transactions_PayPalOrderId",
                table: "paypal_transactions",
                column: "PayPalOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paypal_transactions_PayPalTransactionId",
                table: "paypal_transactions",
                column: "PayPalTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_paypal_transactions_Status",
                table: "paypal_transactions",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_donations_organizaciones_OrganizacionId",
                table: "donations",
                column: "OrganizacionId",
                principalTable: "organizaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_donations_organizaciones_OrganizacionId",
                table: "donations");

            migrationBuilder.DropTable(
                name: "paypal_transactions");

            migrationBuilder.DropIndex(
                name: "IX_donations_OrganizacionId",
                table: "donations");

            migrationBuilder.DropIndex(
                name: "IX_donations_PayPalOrderId",
                table: "donations");

            migrationBuilder.DropIndex(
                name: "IX_donations_PayPalTransactionId",
                table: "donations");

            migrationBuilder.DropColumn(
                name: "SaldoActual",
                table: "organizaciones");

            migrationBuilder.DropColumn(
                name: "EstadoPago",
                table: "donations");

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                table: "donations");

            migrationBuilder.DropColumn(
                name: "OrganizacionId",
                table: "donations");

            migrationBuilder.DropColumn(
                name: "PayPalOrderId",
                table: "donations");

            migrationBuilder.DropColumn(
                name: "PayPalPayerEmail",
                table: "donations");

            migrationBuilder.DropColumn(
                name: "PayPalPayerId",
                table: "donations");

            migrationBuilder.DropColumn(
                name: "PayPalPaymentStatus",
                table: "donations");

            migrationBuilder.DropColumn(
                name: "PayPalTransactionId",
                table: "donations");

            migrationBuilder.AlterColumn<int>(
                name: "FinancialReportId",
                table: "donations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
