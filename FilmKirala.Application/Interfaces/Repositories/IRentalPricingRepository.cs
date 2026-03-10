using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.Interfaces.Repositories
{
    public interface IRentalPricingRepository : IGenericRepository<RentalPricing>
    {
        /// <summary>
        /// Belirli bir filme ait, belirtilen türdeki fiyat paketini Movie dahil çeker.
        /// Kullanıcıdan internal RentalPricingId yerine MovieId + DurationType alınır;
        /// service bu metotla doğru paketi bulur. (Temiz API, veri tutarsızlığı yok)
        /// </summary>
        Task<RentalPricing?> GetPricingByMovieAndTypeAsync(int movieId, DurationType durationType);
    }
}
