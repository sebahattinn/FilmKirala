using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmKirala.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Rentals_MovieId",
                table: "Rentals",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_Status_EndRentalDate",
                table: "Rentals",
                columns: new[] { "Status", "EndRentalDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rentals_MovieId",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_Status_EndRentalDate",
                table: "Rentals");
        }
    }
}
