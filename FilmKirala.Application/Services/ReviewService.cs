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

        public async Task<IEnumerable<MyReviewDto>> GetMyReviewsAsync(int userId)
        {
            var reviews = (await unitOfWork.Reviews.FindAsync(r => r.UserId == userId))
                .OrderByDescending(r => r.Id)
                .ToList();

            if (!reviews.Any()) return Enumerable.Empty<MyReviewDto>();

            var movieIds = reviews.Select(r => r.MovieId).Distinct().ToList();
            var movies = (await unitOfWork.Movies.FindAsync(m => movieIds.Contains(m.Id)))
                .ToDictionary(m => m.Id);

            return reviews.Select(r => new MyReviewDto
            {
                ReviewId  = r.Id,
                MovieId   = r.MovieId,
                MovieTitle = movies.TryGetValue(r.MovieId, out var movie) ? movie.Title : "Unknown",
                Comment   = r.Comment,
                Rating    = (int)r.Rating,
                CreatedAt = r.CreatedAt
            });
        }
    }
}