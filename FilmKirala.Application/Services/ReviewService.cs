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

        public async Task<PagedResult<RatedReviewDto>> GetReviewsByRatingAsync(int rating, int? movieId, int page = 1, int pageSize = 50)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.");

            System.Linq.Expressions.Expression<Func<Review, bool>> predicate = r =>
                (int)r.Rating == rating &&
                (!movieId.HasValue || r.MovieId == movieId.Value);

            var reviews = (await unitOfWork.Reviews.GetPagedAsync(page, pageSize, predicate)).ToList();
            var totalCount = await unitOfWork.Reviews.CountAsync(predicate);

            if (!reviews.Any())
                return new PagedResult<RatedReviewDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };

            var movieIds = reviews.Select(r => r.MovieId).Distinct().ToList();
            var movies = (await unitOfWork.Movies.FindAsync(m => movieIds.Contains(m.Id)))
                .ToDictionary(m => m.Id);

            return new PagedResult<RatedReviewDto>
            {
                Items = reviews.Select(r => new RatedReviewDto
                {
                    ReviewId   = r.Id,
                    MovieId    = r.MovieId,
                    MovieTitle = movies.TryGetValue(r.MovieId, out var m) ? m.Title : "Unknown",
                    Comment    = r.Comment,
                    Rating     = (int)r.Rating,
                    CreatedAt  = r.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}