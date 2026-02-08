using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmKirala.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanUpDatabaseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_RentalPricings_RentalPricingId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_RentalPricingId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RentalPricingId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RentalPriceId",
                table: "Rentals");

            migrationBuilder.AlterColumn<int>(
                name: "RentalPricingId",
                table: "Rentals",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RentalPricingId",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RentalPricingId",
                table: "Rentals",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "RentalPriceId",
                table: "Rentals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_RentalPricingId",
                table: "Reviews",
                column: "RentalPricingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_RentalPricings_RentalPricingId",
                table: "Reviews",
                column: "RentalPricingId",
                principalTable: "RentalPricings",
                principalColumn: "Id");
        }
    }
}
