using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToSubCa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategorySubCategory_Categories_CategoriesId",
                table: "CategorySubCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_CategorySubCategory_SubCategories_SubCategoriesId",
                table: "CategorySubCategory");

            migrationBuilder.RenameColumn(
                name: "SubCategoriesId",
                table: "CategorySubCategory",
                newName: "SubCategoryId");

            migrationBuilder.RenameColumn(
                name: "CategoriesId",
                table: "CategorySubCategory",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_CategorySubCategory_SubCategoriesId",
                table: "CategorySubCategory",
                newName: "IX_CategorySubCategory_SubCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategorySubCategory_Categories_CategoryId",
                table: "CategorySubCategory",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CategorySubCategory_SubCategories_SubCategoryId",
                table: "CategorySubCategory",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategorySubCategory_Categories_CategoryId",
                table: "CategorySubCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_CategorySubCategory_SubCategories_SubCategoryId",
                table: "CategorySubCategory");

            migrationBuilder.RenameColumn(
                name: "SubCategoryId",
                table: "CategorySubCategory",
                newName: "SubCategoriesId");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "CategorySubCategory",
                newName: "CategoriesId");

            migrationBuilder.RenameIndex(
                name: "IX_CategorySubCategory_SubCategoryId",
                table: "CategorySubCategory",
                newName: "IX_CategorySubCategory_SubCategoriesId");

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
        }
    }
}
