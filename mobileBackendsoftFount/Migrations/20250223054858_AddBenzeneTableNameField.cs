using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddBenzeneTableNameField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "rateOfVats",
                table: "Benzenes",
                newName: "RateOfVats");

            migrationBuilder.RenameColumn(
                name: "rateOfTaxes",
                table: "Benzenes",
                newName: "RateOfTaxes");

            migrationBuilder.RenameColumn(
                name: "rateOfEvaporation",
                table: "Benzenes",
                newName: "RateOfEvaporation");

            migrationBuilder.RenameColumn(
                name: "priceOfSelling",
                table: "Benzenes",
                newName: "PriceOfSelling");

            migrationBuilder.RenameColumn(
                name: "priceOfLitre",
                table: "Benzenes",
                newName: "PriceOfLitre");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Benzenes",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Benzenes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Benzenes");

            migrationBuilder.RenameColumn(
                name: "RateOfVats",
                table: "Benzenes",
                newName: "rateOfVats");

            migrationBuilder.RenameColumn(
                name: "RateOfTaxes",
                table: "Benzenes",
                newName: "rateOfTaxes");

            migrationBuilder.RenameColumn(
                name: "RateOfEvaporation",
                table: "Benzenes",
                newName: "rateOfEvaporation");

            migrationBuilder.RenameColumn(
                name: "PriceOfSelling",
                table: "Benzenes",
                newName: "priceOfSelling");

            migrationBuilder.RenameColumn(
                name: "PriceOfLitre",
                table: "Benzenes",
                newName: "priceOfLitre");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Benzenes",
                newName: "id");
        }
    }
}
