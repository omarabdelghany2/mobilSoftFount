using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddSellProductModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SellProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceiveAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RoundOneAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RoundTwoAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RoundThreeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    BoughtAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    BoughtRound = table.Column<int>(type: "integer", nullable: false),
                    SoldAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    OilSupplierId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellProducts_OilSuppliers_OilSupplierId",
                        column: x => x.OilSupplierId,
                        principalTable: "OilSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SellProducts_Name",
                table: "SellProducts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellProducts_OilSupplierId",
                table: "SellProducts",
                column: "OilSupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SellProducts");
        }
    }
}
