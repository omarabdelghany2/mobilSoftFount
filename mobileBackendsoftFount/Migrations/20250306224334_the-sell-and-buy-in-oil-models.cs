using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class thesellandbuyinoilmodels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SellProducts");

            migrationBuilder.CreateTable(
                name: "OilBuyReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Round = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    MonthlyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MonthlyBuyIndex = table.Column<int>(type: "integer", nullable: false),
                    TotalValue = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilBuyReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OilSellRecipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    OilSupplierId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilSellRecipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OilSellRecipes_OilSuppliers_OilSupplierId",
                        column: x => x.OilSupplierId,
                        principalTable: "OilSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OilBuyProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceOfBuy = table.Column<decimal>(type: "numeric", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    OilBuyReceiptId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilBuyProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OilBuyProducts_OilBuyReceipts_OilBuyReceiptId",
                        column: x => x.OilBuyReceiptId,
                        principalTable: "OilBuyReceipts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OilSellProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceiveAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RoundOneAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RoundTwoAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RoundThreeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    BoughtAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    BoughtRound = table.Column<int>(type: "integer", nullable: false),
                    SoldAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    OilSupplierId = table.Column<int>(type: "integer", nullable: false),
                    OilSellRecipeId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilSellProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OilSellProducts_OilSellRecipes_OilSellRecipeId",
                        column: x => x.OilSellRecipeId,
                        principalTable: "OilSellRecipes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OilSellProducts_OilSuppliers_OilSupplierId",
                        column: x => x.OilSupplierId,
                        principalTable: "OilSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OilBuyProducts_OilBuyReceiptId",
                table: "OilBuyProducts",
                column: "OilBuyReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_OilBuyReceipts_Date",
                table: "OilBuyReceipts",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OilSellProducts_Name",
                table: "OilSellProducts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OilSellProducts_OilSellRecipeId",
                table: "OilSellProducts",
                column: "OilSellRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_OilSellProducts_OilSupplierId",
                table: "OilSellProducts",
                column: "OilSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_OilSellRecipes_Date",
                table: "OilSellRecipes",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OilSellRecipes_OilSupplierId",
                table: "OilSellRecipes",
                column: "OilSupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OilBuyProducts");

            migrationBuilder.DropTable(
                name: "OilSellProducts");

            migrationBuilder.DropTable(
                name: "OilBuyReceipts");

            migrationBuilder.DropTable(
                name: "OilSellRecipes");

            migrationBuilder.CreateTable(
                name: "SellProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OilSupplierId = table.Column<int>(type: "integer", nullable: false),
                    BoughtAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    BoughtRound = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceiveAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RoundOneAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RoundThreeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RoundTwoAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    SoldAmount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellProducts_OilSuppliers_OilSupplierId",
                        column: x => x.OilSupplierId,
                        principalTable: "OilSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SellProducts_Name",
                table: "SellProducts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellProducts_OilSupplierId",
                table: "SellProducts",
                column: "OilSupplierId");
        }
    }
}
