using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDeleteForReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DisabilityAndIncreaseReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisabilityAndIncreaseReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportTables1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    day = table.Column<int>(type: "integer", nullable: false),
                    StartingBalanceOfDay = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceivingOfBuyReceipt = table.Column<decimal>(type: "numeric", nullable: false),
                    Calibrations = table.Column<decimal>(type: "numeric", nullable: false),
                    SellingInGunCounters = table.Column<decimal>(type: "numeric", nullable: false),
                    EndingBalanceOfDay = table.Column<decimal>(type: "numeric", nullable: false),
                    DisabilityAndIncreaseReportId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTables1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportTables1_DisabilityAndIncreaseReports_DisabilityAndInc~",
                        column: x => x.DisabilityAndIncreaseReportId,
                        principalTable: "DisabilityAndIncreaseReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportTables2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    day = table.Column<int>(type: "integer", nullable: false),
                    TotalAmountInTankATG = table.Column<decimal>(type: "numeric", nullable: false),
                    WaterAmountInCM = table.Column<decimal>(type: "numeric", nullable: false),
                    WaterAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmountWithoutWater = table.Column<decimal>(type: "numeric", nullable: false),
                    DisabilityAndIncreaseReportId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTables2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportTables2_DisabilityAndIncreaseReports_DisabilityAndInc~",
                        column: x => x.DisabilityAndIncreaseReportId,
                        principalTable: "DisabilityAndIncreaseReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportTables3",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    day = table.Column<int>(type: "integer", nullable: false),
                    DifferenceInAmountATG = table.Column<decimal>(type: "numeric", nullable: false),
                    DifferenceBetweenReceiptAndMeasure = table.Column<decimal>(type: "numeric", nullable: false),
                    DifferencePercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    DisabilityAndIncreaseReportId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTables3", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportTables3_DisabilityAndIncreaseReports_DisabilityAndInc~",
                        column: x => x.DisabilityAndIncreaseReportId,
                        principalTable: "DisabilityAndIncreaseReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportTables4",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    day = table.Column<int>(type: "integer", nullable: false),
                    SellingInGunCountersCumulative = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalCumulative = table.Column<decimal>(type: "numeric", nullable: false),
                    PercentageCumulative = table.Column<decimal>(type: "numeric", nullable: false),
                    DisabilityAndIncreaseReportId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTables4", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportTables4_DisabilityAndIncreaseReports_DisabilityAndInc~",
                        column: x => x.DisabilityAndIncreaseReportId,
                        principalTable: "DisabilityAndIncreaseReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportTables1_DisabilityAndIncreaseReportId",
                table: "ReportTables1",
                column: "DisabilityAndIncreaseReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTables2_DisabilityAndIncreaseReportId",
                table: "ReportTables2",
                column: "DisabilityAndIncreaseReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTables3_DisabilityAndIncreaseReportId",
                table: "ReportTables3",
                column: "DisabilityAndIncreaseReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTables4_DisabilityAndIncreaseReportId",
                table: "ReportTables4",
                column: "DisabilityAndIncreaseReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportTables1");

            migrationBuilder.DropTable(
                name: "ReportTables2");

            migrationBuilder.DropTable(
                name: "ReportTables3");

            migrationBuilder.DropTable(
                name: "ReportTables4");

            migrationBuilder.DropTable(
                name: "DisabilityAndIncreaseReports");
        }
    }
}
