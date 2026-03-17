using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using FilmKirala.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FilmKirala.Infrastructure.Repositories
{
    public class RentalPricingRepository : GenericRepository<RentalPricing>, IRentalPricingRepository
    {
        public RentalPricingRepository(AppDbContext context, ILogger<RentalPricingRepository> logger)
            : base(context, logger) { }

        public async Task<RentalPricing?> GetPricingByMovieAndTypeAsync(int movieId, DurationType durationType)
        {
            return await _context.RentalPricings
                .Include(p => p.Movie)
                .AsTracking()
                .FirstOrDefaultAsync(p => p.MovieId == movieId && p.DurationType == durationType);
        }
    }
}
