using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddBenzeneBuyReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BenzeneBuyReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncrementalId = table.Column<int>(type: "integer", nullable: false),
                    MobilId = table.Column<int>(type: "integer", nullable: false),
                    MobilReceiptDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenzeneBuyReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BenzeneRecipeProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Amount = table.Column<float>(type: "real", nullable: false),
                    PricePerLiter = table.Column<float>(type: "real", nullable: false),
                    EvaporationPercentage = table.Column<float>(type: "real", nullable: false),
                    Taxes = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenzeneRecipeProducts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BenzeneBuyReceipts");

            migrationBuilder.DropTable(
                name: "BenzeneRecipeProducts");
        }
    }
}
