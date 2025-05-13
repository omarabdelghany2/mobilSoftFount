using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceSellReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceSellReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSellReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceSellProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    ClientCarModel = table.Column<string>(type: "text", nullable: false),
                    ClientCarNumber = table.Column<string>(type: "text", nullable: false),
                    Worker = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    ServiceSellReceiptId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSellProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceSellProducts_ServiceSellReceipts_ServiceSellReceiptId",
                        column: x => x.ServiceSellReceiptId,
                        principalTable: "ServiceSellReceipts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    SubCategoryName = table.Column<string>(type: "text", nullable: false),
                    SubCategoryPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ServiceSellProductId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientServices_ServiceSellProducts_ServiceSellProductId",
                        column: x => x.ServiceSellProductId,
                        principalTable: "ServiceSellProducts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientServices_ServiceSellProductId",
                table: "ClientServices",
                column: "ServiceSellProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSellProducts_ServiceSellReceiptId",
                table: "ServiceSellProducts",
                column: "ServiceSellReceiptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientServices");

            migrationBuilder.DropTable(
                name: "ServiceSellProducts");

            migrationBuilder.DropTable(
                name: "ServiceSellReceipts");
        }
    }
}
