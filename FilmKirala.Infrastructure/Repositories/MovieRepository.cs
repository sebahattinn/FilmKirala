using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Domain.Entity;
using FilmKirala.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; 

namespace FilmKirala.Infrastructure.Repositories
{
    public class MovieRepository : GenericRepository<Movie>, IMovieRepository
    {
        public MovieRepository(AppDbContext context, ILogger<MovieRepository> logger) : base(context, logger) { }

        public async Task<Movie?> GetMovieWithDetailsAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.RentalPricings)
                .Include(m => m.Reviews)
                    .ThenInclude(r => r.User)
                    .AsTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}