using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IMovieRepository Movies { get; }
        IUserRepository Users { get; }
        IGenericRepository<Rental> Rentals { get; } 
        IGenericRepository<Review> Reviews { get; }
        IGenericRepository<RentalPricing> RentalPricings { get; }
        Task<int> CompleteAsync();  // Veritabanına kaydetme metodu teamplate'i
    }
}