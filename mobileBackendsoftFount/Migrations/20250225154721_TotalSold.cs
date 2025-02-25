using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class TotalSold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TotalSold",
                table: "BenzeneGunCounters",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalSold",
                table: "BenzeneGunCounters");
        }
    }
}
