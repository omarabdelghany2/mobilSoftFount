using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceClientPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceSellProducts_ServiceSellReceipts_ServiceSellReceiptId",
                table: "ServiceSellProducts");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceSellReceiptId",
                table: "ServiceSellProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientPhone",
                table: "ServiceSellProducts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceSellProducts_ServiceSellReceipts_ServiceSellReceiptId",
                table: "ServiceSellProducts",
                column: "ServiceSellReceiptId",
                principalTable: "ServiceSellReceipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceSellProducts_ServiceSellReceipts_ServiceSellReceiptId",
                table: "ServiceSellProducts");

            migrationBuilder.DropColumn(
                name: "ClientPhone",
                table: "ServiceSellProducts");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceSellReceiptId",
                table: "ServiceSellProducts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceSellProducts_ServiceSellReceipts_ServiceSellReceiptId",
                table: "ServiceSellProducts",
                column: "ServiceSellReceiptId",
                principalTable: "ServiceSellReceipts",
                principalColumn: "Id");
        }
    }
}
