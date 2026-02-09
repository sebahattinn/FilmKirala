using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task AddReviewAsync(CreateReviewDto createReviewDto, int userId);
    }
}