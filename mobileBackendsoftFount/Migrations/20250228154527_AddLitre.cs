using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddLitre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalLiter95",
                table: "SellingReceipts",
                newName: "TotalLitre95");

            migrationBuilder.RenameColumn(
                name: "TotalLiter92",
                table: "SellingReceipts",
                newName: "TotalLitre92");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalLitre95",
                table: "SellingReceipts",
                newName: "TotalLiter95");

            migrationBuilder.RenameColumn(
                name: "TotalLitre92",
                table: "SellingReceipts",
                newName: "TotalLiter92");
        }
    }
}
