using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToMonthlyDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MinistryOfSupplyLetters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Introduction = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MonthlyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinistryOfSupplyLetters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MinistryOfSupplyLetterMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    SoldAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalSoldMoney = table.Column<decimal>(type: "numeric", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IncomeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    MinistryOfSupplyLetterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinistryOfSupplyLetterMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MinistryOfSupplyLetterMembers_MinistryOfSupplyLetters_Minis~",
                        column: x => x.MinistryOfSupplyLetterId,
                        principalTable: "MinistryOfSupplyLetters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BenzeneBuyReceipts_MobilReceiptDate",
                table: "BenzeneBuyReceipts",
                column: "MobilReceiptDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MinistryOfSupplyLetterMembers_MinistryOfSupplyLetterId",
                table: "MinistryOfSupplyLetterMembers",
                column: "MinistryOfSupplyLetterId");

            migrationBuilder.CreateIndex(
                name: "IX_MinistryOfSupplyLetters_MonthlyDate",
                table: "MinistryOfSupplyLetters",
                column: "MonthlyDate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MinistryOfSupplyLetterMembers");

            migrationBuilder.DropTable(
                name: "MinistryOfSupplyLetters");

            migrationBuilder.DropIndex(
                name: "IX_BenzeneBuyReceipts_MobilReceiptDate",
                table: "BenzeneBuyReceipts");
        }
    }
}
