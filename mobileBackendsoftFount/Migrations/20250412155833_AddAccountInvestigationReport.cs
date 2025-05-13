using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountInvestigationReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountInvestigationReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BalanceOfStart = table.Column<float>(type: "real", nullable: false),
                    TotalBuyReceiptMoney = table.Column<float>(type: "real", nullable: false),
                    TotalDeposit = table.Column<float>(type: "real", nullable: false),
                    TotalAdjustmentMoney = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountInvestigationReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountInvestigationMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ReciptTotalMoney = table.Column<float>(type: "real", nullable: false),
                    Benzene95Litre = table.Column<float>(type: "real", nullable: false),
                    Benzene92Litre = table.Column<float>(type: "real", nullable: false),
                    DepostMoney = table.Column<float>(type: "real", nullable: false),
                    EvaporationMoney = table.Column<float>(type: "real", nullable: false),
                    VatesMoney = table.Column<float>(type: "real", nullable: false),
                    Taxes95 = table.Column<float>(type: "real", nullable: false),
                    Taxes92 = table.Column<float>(type: "real", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    Balance = table.Column<float>(type: "real", nullable: false),
                    AccountInvestigationReportId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountInvestigationMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountInvestigationMembers_AccountInvestigationReports_Acc~",
                        column: x => x.AccountInvestigationReportId,
                        principalTable: "AccountInvestigationReports",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountInvestigationMembers_AccountInvestigationReportId",
                table: "AccountInvestigationMembers",
                column: "AccountInvestigationReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountInvestigationMembers");

            migrationBuilder.DropTable(
                name: "AccountInvestigationReports");
        }
    }
}
