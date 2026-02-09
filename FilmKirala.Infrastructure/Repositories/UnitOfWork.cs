using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Domain.Entity;
using FilmKirala.Infrastructure.Persistence;

namespace FilmKirala.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IMovieRepository Movies { get; private set; }
        public IUserRepository Users { get; private set; }
        public IGenericRepository<Rental> Rentals { get; private set; }
        public IGenericRepository<Review> Reviews { get; private set; }
        public IGenericRepository<RentalPricing> RentalPricings { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            // Repository'leri burada new'liyoruz   çok hoşuma gitmedi de başka türlü beceremedim valla
            Movies = new MovieRepository(_context);
            Users = new UserRepository(_context);
            Rentals = new GenericRepository<Rental>(_context);
            Reviews = new GenericRepository<Review>(_context);
            RentalPricings = new GenericRepository<RentalPricing>(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}