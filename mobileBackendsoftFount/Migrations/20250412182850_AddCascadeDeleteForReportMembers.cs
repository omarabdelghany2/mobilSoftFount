using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDeleteForReportMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountInvestigationMembers_AccountInvestigationReports_Acc~",
                table: "AccountInvestigationMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountInvestigationMembers_AccountInvestigationReports_Acc~",
                table: "AccountInvestigationMembers",
                column: "AccountInvestigationReportId",
                principalTable: "AccountInvestigationReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountInvestigationMembers_AccountInvestigationReports_Acc~",
                table: "AccountInvestigationMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountInvestigationMembers_AccountInvestigationReports_Acc~",
                table: "AccountInvestigationMembers",
                column: "AccountInvestigationReportId",
                principalTable: "AccountInvestigationReports",
                principalColumn: "Id");
        }
    }
}
