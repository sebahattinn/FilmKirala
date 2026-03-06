using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.Services
{
    public class ReviewService(IUnitOfWork unitOfWork) : IReviewService
    {
      
        public async Task AddReviewAsync(CreateReviewDto createReviewDto, int userId)
        {
            _ = await unitOfWork.Movies.GetByIdAsync(createReviewDto.MovieId)
                ?? throw new KeyNotFoundException($"'{createReviewDto.MovieId}' ID'li film not founded!");

            var review = new Review(
                userId,
                createReviewDto.MovieId,
                createReviewDto.Comment,
                (Rating)createReviewDto.Rating
            );

            await unitOfWork.Reviews.AddAsync(review);
            await unitOfWork.CompleteAsync();
        }
    }
}