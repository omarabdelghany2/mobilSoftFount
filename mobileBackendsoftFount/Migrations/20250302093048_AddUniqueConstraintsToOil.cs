using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsToOil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OilSuppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilSuppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Oils",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<float>(type: "real", nullable: false),
                    PriceOfSelling = table.Column<float>(type: "real", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oils", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oils_OilSuppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "OilSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Oils_Name",
                table: "Oils",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Oils_Order",
                table: "Oils",
                column: "Order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Oils_SupplierId",
                table: "Oils",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_OilSuppliers_Name",
                table: "OilSuppliers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Oils");

            migrationBuilder.DropTable(
                name: "OilSuppliers");
        }
    }
}
