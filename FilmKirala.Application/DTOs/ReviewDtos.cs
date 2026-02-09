using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.DTOs
{

    public record CreateReviewDto(
        int MovieId,
        string Comment,
        Rating Rating
    );
  
    public record ReviewDto(
        int Id,
        string Username,
        string Comment,
        int Rating,
        DateTime CreatedAt
    );
}