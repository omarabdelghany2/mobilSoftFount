using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class FixOilSellProductRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OilSellProducts_OilSellRecipes_OilSellRecipeId",
                table: "OilSellProducts");

            migrationBuilder.AlterColumn<int>(
                name: "OilSellRecipeId",
                table: "OilSellProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OilSellProducts_OilSellRecipes_OilSellRecipeId",
                table: "OilSellProducts",
                column: "OilSellRecipeId",
                principalTable: "OilSellRecipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OilSellProducts_OilSellRecipes_OilSellRecipeId",
                table: "OilSellProducts");

            migrationBuilder.AlterColumn<int>(
                name: "OilSellRecipeId",
                table: "OilSellProducts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_OilSellProducts_OilSellRecipes_OilSellRecipeId",
                table: "OilSellProducts",
                column: "OilSellRecipeId",
                principalTable: "OilSellRecipes",
                principalColumn: "Id");
        }
    }
}
