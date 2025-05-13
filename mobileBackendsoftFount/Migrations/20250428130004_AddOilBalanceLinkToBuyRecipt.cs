using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddOilBalanceLinkToBuyRecipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OilAccountBalanceId",
                table: "OilBuyReceipts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OilBuyReceipts_OilAccountBalanceId",
                table: "OilBuyReceipts",
                column: "OilAccountBalanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_OilBuyReceipts_oilAccountBalances_OilAccountBalanceId",
                table: "OilBuyReceipts",
                column: "OilAccountBalanceId",
                principalTable: "oilAccountBalances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OilBuyReceipts_oilAccountBalances_OilAccountBalanceId",
                table: "OilBuyReceipts");

            migrationBuilder.DropIndex(
                name: "IX_OilBuyReceipts_OilAccountBalanceId",
                table: "OilBuyReceipts");

            migrationBuilder.DropColumn(
                name: "OilAccountBalanceId",
                table: "OilBuyReceipts");
        }
    }
}
