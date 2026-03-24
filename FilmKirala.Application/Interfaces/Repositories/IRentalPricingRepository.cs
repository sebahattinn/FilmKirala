using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.Interfaces.Repositories
{
    public interface IRentalPricingRepository : IGenericRepository<RentalPricing>
    {
   
        Task<RentalPricing?> GetPricingByMovieAndTypeAsync(int movieId, DurationType durationType);
    }
}
