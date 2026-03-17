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
                ?? throw new KeyNotFoundException($"Film not found (ID: {createReviewDto.MovieId})");

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

        public async Task<IEnumerable<RatedReviewDto>> GetReviewsByRatingAsync(int rating, int? movieId)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.");

            var reviews = (await unitOfWork.Reviews.FindAsync(r =>
                    (int)r.Rating == rating &&
                    (!movieId.HasValue || r.MovieId == movieId.Value)))
                .OrderByDescending(r => r.Id)
                .ToList();

            if (!reviews.Any()) return Enumerable.Empty<RatedReviewDto>();

            var movieIds = reviews.Select(r => r.MovieId).Distinct().ToList();
            var movies = (await unitOfWork.Movies.FindAsync(m => movieIds.Contains(m.Id)))
                .ToDictionary(m => m.Id);

            return reviews.Select(r => new RatedReviewDto
            {
                ReviewId   = r.Id,
                MovieId    = r.MovieId,
                MovieTitle = movies.TryGetValue(r.MovieId, out var m) ? m.Title : "Unknown",
                Comment    = r.Comment,
                Rating     = (int)r.Rating,
                CreatedAt  = r.CreatedAt
            });
        }
    }
}