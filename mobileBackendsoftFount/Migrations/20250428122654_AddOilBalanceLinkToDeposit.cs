using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddOilBalanceLinkToDeposit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OilBalanceId",
                table: "OilDeposits",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OilDeposits_OilBalanceId",
                table: "OilDeposits",
                column: "OilBalanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_OilDeposits_oilAccountBalances_OilBalanceId",
                table: "OilDeposits",
                column: "OilBalanceId",
                principalTable: "oilAccountBalances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OilDeposits_oilAccountBalances_OilBalanceId",
                table: "OilDeposits");

            migrationBuilder.DropIndex(
                name: "IX_OilDeposits_OilBalanceId",
                table: "OilDeposits");

            migrationBuilder.DropColumn(
                name: "OilBalanceId",
                table: "OilDeposits");
        }
    }
}
