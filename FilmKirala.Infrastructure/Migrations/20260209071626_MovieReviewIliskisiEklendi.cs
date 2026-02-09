using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmKirala.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MovieReviewIliskisiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MovieId1",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MovieId1",
                table: "Reviews",
                column: "MovieId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Movies_MovieId1",
                table: "Reviews",
                column: "MovieId1",
                principalTable: "Movies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Movies_MovieId1",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_MovieId1",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "MovieId1",
                table: "Reviews");
        }
    }
}
