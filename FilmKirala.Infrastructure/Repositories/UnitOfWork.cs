using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Domain.Entity;
using FilmKirala.Infrastructure.Persistence;

namespace FilmKirala.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private bool _disposed;

        public UnitOfWork(AppDbContext context,
                          IMovieRepository movieRepository,
                          IUserRepository userRepository)
        {
            _context = context;
            Movies = movieRepository;
            Users = userRepository;
            Rentals = new GenericRepository<Rental>(_context);
            Reviews = new GenericRepository<Review>(_context);
            RentalPricings = new GenericRepository<RentalPricing>(_context);
        }

        public IMovieRepository Movies { get; }
        public IUserRepository Users { get; }
        public IGenericRepository<Rental> Rentals { get; }
        public IGenericRepository<Review> Reviews { get; }
        public IGenericRepository<RentalPricing> RentalPricings { get; }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
      
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}