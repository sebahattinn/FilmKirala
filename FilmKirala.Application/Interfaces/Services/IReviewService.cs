using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task AddReviewAsync(CreateReviewDto createReviewDto, int userId);
        Task<IEnumerable<MyReviewDto>> GetMyReviewsAsync(int userId);
        Task<IEnumerable<RatedReviewDto>> GetReviewsByRatingAsync(int rating, int? movieId);
    }
}