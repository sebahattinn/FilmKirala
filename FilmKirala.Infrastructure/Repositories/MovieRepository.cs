using Dapper;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using FilmKirala.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FilmKirala.Infrastructure.Repositories
{
    public class MovieRepository : GenericRepository<Movie>, IMovieRepository
    {
        // Three simple, focused queries sent in a single round-trip via QueryMultiple.
        // SQL Server compiles each plan independently (fast) — no multi-table JOIN, no Cartesian product.
        // Only the columns actually needed by MovieDetailDto are selected; full User rows are never loaded.
        private const string MovieDetailSql = """
            SELECT Id, Title, Description, Genre, Stock, IsActive
            FROM   Movies
            WHERE  Id = @id;

            SELECT Id, DurationType, DurationValue, Price
            FROM   RentalPricings
            WHERE  MovieId = @id;

            SELECT r.Id, r.Rating, r.Comment, r.CreatedAt, u.Username
            FROM   Reviews r
            INNER JOIN Users u ON u.Id = r.UserId
            WHERE  r.MovieId = @id
            ORDER BY r.Id DESC;
            """;

        public MovieRepository(AppDbContext context, ILogger<MovieRepository> logger)
            : base(context, logger) { }

        public async Task<MovieDetailDto?> GetMovieWithDetailsAsync(int id)
        {
            // New connection from the same connection string EF Core uses — pool is shared.
            await using var conn = new SqlConnection(_context.Database.GetConnectionString());

            using var multi = await conn.QueryMultipleAsync(MovieDetailSql, new { id });

            var movie = await multi.ReadFirstOrDefaultAsync<MovieRow>();
            if (movie is null) return null;

            var pricings = (await multi.ReadAsync<PricingRow>()).ToList();
            var reviews  = (await multi.ReadAsync<ReviewRow>()).ToList();

            return new MovieDetailDto
            {
                Id          = movie.Id,
                Title       = movie.Title,
                Description = movie.Description,
                Genre       = movie.Genre,
                Stock       = movie.Stock,
                IsActive    = movie.IsActive,

                RentalOptions = pricings.Select(p => new PricingDto
                {
                    Id            = p.Id,
                    DurationType  = p.DurationType,
                    DurationValue = p.DurationValue,
                    Price         = p.Price
                }).ToList(),

                Reviews = reviews.Select(r => new ReviewDto
                {
                    Id        = r.Id,
                    Username  = r.Username,
                    Comment   = r.Comment,
                    Rating    = r.Rating,
                    CreatedAt = r.CreatedAt
                }).ToList(),

                AverageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0
            };
        }

        // Private Dapper result types — never exposed outside this class.
        // Must be classes with public setters so Dapper maps by column name (case-insensitive),
        // not by constructor parameter position. Positional records fail when SQL column
        // order doesn't exactly match the constructor signature.
        private sealed class MovieRow
        {
            public int Id { get; set; }
            public string Title { get; set; } = null!;
            public string Description { get; set; } = null!;
            public string Genre { get; set; } = null!;
            public int Stock { get; set; }
            public bool IsActive { get; set; }
        }

        private sealed class PricingRow
        {
            public int Id { get; set; }
            public DurationType DurationType { get; set; }
            public int DurationValue { get; set; }
            public int Price { get; set; }
        }

        private sealed class ReviewRow
        {
            public int Id { get; set; }
            public string Username { get; set; } = null!;
            public string Comment { get; set; } = null!;
            public int Rating { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
