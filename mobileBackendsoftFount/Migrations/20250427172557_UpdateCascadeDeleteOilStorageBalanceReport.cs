using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCascadeDeleteOilStorageBalanceReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OilStorageBalanceReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    StoragePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    StoragePriceOfSell = table.Column<decimal>(type: "numeric", nullable: false),
                    Profit = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilStorageBalanceReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OilBalanceProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierName = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceOfSell = table.Column<decimal>(type: "numeric", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    OilStorageBalanceReportId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilBalanceProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OilBalanceProducts_OilStorageBalanceReports_OilStorageBalan~",
                        column: x => x.OilStorageBalanceReportId,
                        principalTable: "OilStorageBalanceReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OilBalanceProducts_OilStorageBalanceReportId",
                table: "OilBalanceProducts",
                column: "OilStorageBalanceReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OilBalanceProducts");

            migrationBuilder.DropTable(
                name: "OilStorageBalanceReports");
        }
    }
}
