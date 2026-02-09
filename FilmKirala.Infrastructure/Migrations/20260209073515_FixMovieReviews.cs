using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmKirala.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMovieReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // KANKA DİKKAT: Sen SQL'den elle sildiğin için bu kodları yorum satırına aldık.
            // Böylece EF Core hata vermeden "Tamam hallettim" diyip geçecek.

            /*
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Movies_MovieId1",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_MovieId1",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "MovieId1",
                table: "Reviews");
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Burası geri alma (Rollback) senaryosu. Eğer ilerde bu migration'ı geri alırsan
            // o saçma kolonları tekrar oluşturur. Şimdilik zararı yok, kalsın.
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
    }
}