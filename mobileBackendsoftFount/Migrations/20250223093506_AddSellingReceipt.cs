using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddSellingReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SellingReceiptId",
                table: "BenzeneGunCounters",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SellingReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellingReceipts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BenzeneGunCounters_SellingReceiptId",
                table: "BenzeneGunCounters",
                column: "SellingReceiptId");

            migrationBuilder.AddForeignKey(
                name: "FK_BenzeneGunCounters_SellingReceipts_SellingReceiptId",
                table: "BenzeneGunCounters",
                column: "SellingReceiptId",
                principalTable: "SellingReceipts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BenzeneGunCounters_SellingReceipts_SellingReceiptId",
                table: "BenzeneGunCounters");

            migrationBuilder.DropTable(
                name: "SellingReceipts");

            migrationBuilder.DropIndex(
                name: "IX_BenzeneGunCounters_SellingReceiptId",
                table: "BenzeneGunCounters");

            migrationBuilder.DropColumn(
                name: "SellingReceiptId",
                table: "BenzeneGunCounters");
        }
    }
}
