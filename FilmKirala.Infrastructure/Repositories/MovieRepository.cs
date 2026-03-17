using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Domain.Entity;
using FilmKirala.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FilmKirala.Infrastructure.Repositories
{
    public class MovieRepository : GenericRepository<Movie>, IMovieRepository
    {
        // Compiled at class-load time — LINQ-to-SQL translation runs once, never on a user request.
        // Without this, EF Core compiles the expression tree on the very first call (10-30s cold start).
        private static readonly Func<AppDbContext, int, Task<Movie?>> _getMovieWithDetails =
            EF.CompileAsyncQuery((AppDbContext ctx, int id) =>
                ctx.Movies
                    .AsNoTracking()
                    .Include(m => m.RentalPricings)
                    .Include(m => m.Reviews)
                        .ThenInclude(r => r.User)
                    .FirstOrDefault(m => m.Id == id));

        public MovieRepository(AppDbContext context, ILogger<MovieRepository> logger) : base(context, logger) { }

        public Task<Movie?> GetMovieWithDetailsAsync(int id) => _getMovieWithDetails(_context, id);
    }
}
