using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IMovieRepository Movies { get; }
        IUserRepository Users { get; }
        IGenericRepository<Rental> Rentals { get; } // Özel metoda ihtiyaç yoksa Generic yeter
        IGenericRepository<Review> Reviews { get; }
        IGenericRepository<RentalPricing> RentalPricings { get; }

        // Veritabanına kaydetme komutanı
        Task<int> CompleteAsync();
    }
}