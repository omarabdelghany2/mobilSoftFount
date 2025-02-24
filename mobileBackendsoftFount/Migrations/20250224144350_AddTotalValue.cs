using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "TotalValue",
                table: "BenzeneRecipeProducts",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "ValueOfEvaporation",
                table: "BenzeneRecipeProducts",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "ValueOfTaxes",
                table: "BenzeneRecipeProducts",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "TotalValue",
                table: "BenzeneBuyReceipts",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalValue",
                table: "BenzeneRecipeProducts");

            migrationBuilder.DropColumn(
                name: "ValueOfEvaporation",
                table: "BenzeneRecipeProducts");

            migrationBuilder.DropColumn(
                name: "ValueOfTaxes",
                table: "BenzeneRecipeProducts");

            migrationBuilder.DropColumn(
                name: "TotalValue",
                table: "BenzeneBuyReceipts");
        }
    }
}
