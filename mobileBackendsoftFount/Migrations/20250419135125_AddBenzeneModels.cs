using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mobileBackendsoftFount.Migrations
{
    /// <inheritdoc />
    public partial class AddBenzeneModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BenzeneCalibrations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    amount92 = table.Column<double>(type: "double precision", nullable: false),
                    TotalMoney92 = table.Column<double>(type: "double precision", nullable: false),
                    amount95 = table.Column<double>(type: "double precision", nullable: false),
                    TotalMoney95 = table.Column<double>(type: "double precision", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenzeneCalibrations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "BenzeneTanks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tankOne92ATG = table.Column<double>(type: "double precision", nullable: false),
                    tankTwo92ATG = table.Column<double>(type: "double precision", nullable: false),
                    tankOne95ATG = table.Column<double>(type: "double precision", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenzeneTanks", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BenzeneCalibrations_date",
                table: "BenzeneCalibrations",
                column: "date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BenzeneTanks_date",
                table: "BenzeneTanks",
                column: "date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BenzeneCalibrations");

            migrationBuilder.DropTable(
                name: "BenzeneTanks");
        }
    }
}
