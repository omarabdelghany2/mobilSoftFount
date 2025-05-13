using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueDateConstraintsForAdjustmentAndDeposit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BenzeneDeposits_date",
                table: "BenzeneDeposits",
                column: "date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BenzeneAdjustments_date",
                table: "BenzeneAdjustments",
                column: "date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BenzeneDeposits_date",
                table: "BenzeneDeposits");

            migrationBuilder.DropIndex(
                name: "IX_BenzeneAdjustments_date",
                table: "BenzeneAdjustments");
        }
    }
}
