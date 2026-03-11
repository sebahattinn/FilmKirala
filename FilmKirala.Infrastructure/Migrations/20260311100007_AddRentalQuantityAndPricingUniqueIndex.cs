using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmKirala.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalQuantityAndPricingUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RentalPricings_MovieId",
                table: "RentalPricings");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Rentals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RentalPricings_MovieId_DurationType",
                table: "RentalPricings",
                columns: new[] { "MovieId", "DurationType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RentalPricings_MovieId_DurationType",
                table: "RentalPricings");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Rentals");

            migrationBuilder.CreateIndex(
                name: "IX_RentalPricings_MovieId",
                table: "RentalPricings",
                column: "MovieId");
        }
    }
}
