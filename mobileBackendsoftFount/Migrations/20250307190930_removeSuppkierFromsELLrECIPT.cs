using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class removeSuppkierFromsELLrECIPT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OilSellRecipes_OilSuppliers_OilSupplierId",
                table: "OilSellRecipes");

            migrationBuilder.DropIndex(
                name: "IX_OilSellRecipes_OilSupplierId",
                table: "OilSellRecipes");

            migrationBuilder.DropColumn(
                name: "OilSupplierId",
                table: "OilSellRecipes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OilSupplierId",
                table: "OilSellRecipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OilSellRecipes_OilSupplierId",
                table: "OilSellRecipes",
                column: "OilSupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_OilSellRecipes_OilSuppliers_OilSupplierId",
                table: "OilSellRecipes",
                column: "OilSupplierId",
                principalTable: "OilSuppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
