using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.DTOs
{

    public record CreateReviewDto(
        int MovieId,
        string Comment,
        Rating Rating
    );
  
    // Film Detayında Yorumları Listelerken (MovieDetailDto içinde kullanılacak)
    public record ReviewDto(
        int Id,
        string Username, // User Id değil, görünen isim lazım
        string Comment,
        int Rating,
        DateTime CreatedAt
    );
}