using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToBenzene : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalSold95",
                table: "SellingReceipts",
                newName: "TotalMoney95");

            migrationBuilder.RenameColumn(
                name: "TotalSold92",
                table: "SellingReceipts",
                newName: "TotalMoney92");

            migrationBuilder.AddColumn<long>(
                name: "TotalLiter92",
                table: "SellingReceipts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TotalLiter95",
                table: "SellingReceipts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TotalMoney",
                table: "SellingReceipts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Benzenes_Name",
                table: "Benzenes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Benzenes_Name",
                table: "Benzenes");

            migrationBuilder.DropColumn(
                name: "TotalLiter92",
                table: "SellingReceipts");

            migrationBuilder.DropColumn(
                name: "TotalLiter95",
                table: "SellingReceipts");

            migrationBuilder.DropColumn(
                name: "TotalMoney",
                table: "SellingReceipts");

            migrationBuilder.RenameColumn(
                name: "TotalMoney95",
                table: "SellingReceipts",
                newName: "TotalSold95");

            migrationBuilder.RenameColumn(
                name: "TotalMoney92",
                table: "SellingReceipts",
                newName: "TotalSold92");
        }
    }
}
