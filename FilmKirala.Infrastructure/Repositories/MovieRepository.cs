using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Domain.Entity;
using FilmKirala.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FilmKirala.Infrastructure.Repositories
{
    public class MovieRepository : GenericRepository<Movie>, IMovieRepository
    {
        public MovieRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Movie> GetMovieWithDetailsAsync(int id)
        {
            // Filmi getirirken Fiyatlarını ve Yorumlarını da içine doldur (Eager Loading)
            return await _context.Movies
                .Include(m => m.RentalPricings)
                //.Include(m => m.Reviews) // Eğer Review ilişkisi tamamsa bunu açabilirsin
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}