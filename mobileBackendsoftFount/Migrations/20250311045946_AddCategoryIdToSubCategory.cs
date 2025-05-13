using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToSubCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategorySubCategories_Categories_CategoriesId",
                table: "CategorySubCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_CategorySubCategories_SubCategories_SubCategoriesId",
                table: "CategorySubCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategorySubCategories",
                table: "CategorySubCategories");

            migrationBuilder.RenameTable(
                name: "CategorySubCategories",
                newName: "CategorySubCategory");

            migrationBuilder.RenameIndex(
                name: "IX_CategorySubCategories_SubCategoriesId",
                table: "CategorySubCategory",
                newName: "IX_CategorySubCategory_SubCategoriesId");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "SubCategories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "SubCategories",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PriceOfBuy",
                table: "SubCategories",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategorySubCategory",
                table: "CategorySubCategory",
                columns: new[] { "CategoriesId", "SubCategoriesId" });

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CategoryId",
                table: "SubCategories",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategorySubCategory_Categories_CategoriesId",
                table: "CategorySubCategory",
                column: "CategoriesId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CategorySubCategory_SubCategories_SubCategoriesId",
                table: "CategorySubCategory",
                column: "SubCategoriesId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubCategories_Categories_CategoryId",
                table: "SubCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategorySubCategory_Categories_CategoriesId",
                table: "CategorySubCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_CategorySubCategory_SubCategories_SubCategoriesId",
                table: "CategorySubCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_SubCategories_Categories_CategoryId",
                table: "SubCategories");

            migrationBuilder.DropIndex(
                name: "IX_SubCategories_CategoryId",
                table: "SubCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategorySubCategory",
                table: "CategorySubCategory");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "SubCategories");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "SubCategories");

            migrationBuilder.DropColumn(
                name: "PriceOfBuy",
                table: "SubCategories");

            migrationBuilder.RenameTable(
                name: "CategorySubCategory",
                newName: "CategorySubCategories");

            migrationBuilder.RenameIndex(
                name: "IX_CategorySubCategory_SubCategoriesId",
                table: "CategorySubCategories",
                newName: "IX_CategorySubCategories_SubCategoriesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategorySubCategories",
                table: "CategorySubCategories",
                columns: new[] { "CategoriesId", "SubCategoriesId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CategorySubCategories_Categories_CategoriesId",
                table: "CategorySubCategories",
                column: "CategoriesId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CategorySubCategories_SubCategories_SubCategoriesId",
                table: "CategorySubCategories",
                column: "SubCategoriesId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
