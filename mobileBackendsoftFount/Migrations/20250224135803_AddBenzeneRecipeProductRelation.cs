using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddBenzeneRecipeProductRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BenzeneBuyReceiptId",
                table: "BenzeneRecipeProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BenzeneRecipeProducts_BenzeneBuyReceiptId",
                table: "BenzeneRecipeProducts",
                column: "BenzeneBuyReceiptId");

            migrationBuilder.AddForeignKey(
                name: "FK_BenzeneRecipeProducts_BenzeneBuyReceipts_BenzeneBuyReceiptId",
                table: "BenzeneRecipeProducts",
                column: "BenzeneBuyReceiptId",
                principalTable: "BenzeneBuyReceipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BenzeneRecipeProducts_BenzeneBuyReceipts_BenzeneBuyReceiptId",
                table: "BenzeneRecipeProducts");

            migrationBuilder.DropIndex(
                name: "IX_BenzeneRecipeProducts_BenzeneBuyReceiptId",
                table: "BenzeneRecipeProducts");

            migrationBuilder.DropColumn(
                name: "BenzeneBuyReceiptId",
                table: "BenzeneRecipeProducts");
        }
    }
}
