using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class openAmount92and95 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpenAmount",
                table: "SellingReceipts",
                newName: "OpenAmount95");

            migrationBuilder.AddColumn<long>(
                name: "OpenAmount92",
                table: "SellingReceipts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenAmount92",
                table: "SellingReceipts");

            migrationBuilder.RenameColumn(
                name: "OpenAmount95",
                table: "SellingReceipts",
                newName: "OpenAmount");
        }
    }
}
