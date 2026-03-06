using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Domain.Entity;
using FilmKirala.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace FilmKirala.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly ILoggerFactory _loggerFactory; // Logger fabrikası eklendi
        private bool _disposed;

        public UnitOfWork(AppDbContext context,
                          IMovieRepository movieRepository,
                          IUserRepository userRepository,
                          ILoggerFactory loggerFactory) // DI üzerinden fabrikaya ulaşıyoruz
        {
            _context = context;
            _loggerFactory = loggerFactory;
            Movies = movieRepository;
            Users = userRepository;

            // Her bir generic repository için ilgili tipte logger oluşturup içeri gönderiyoruz
            Rentals = new GenericRepository<Rental>(_context, _loggerFactory.CreateLogger<GenericRepository<Rental>>());
            Reviews = new GenericRepository<Review>(_context, _loggerFactory.CreateLogger<GenericRepository<Review>>());
            RentalPricings = new GenericRepository<RentalPricing>(_context, _loggerFactory.CreateLogger<GenericRepository<RentalPricing>>());
            NotificationLogs = new GenericRepository<NotificationLog>(_context, _loggerFactory.CreateLogger<GenericRepository<NotificationLog>>());
        }

        public IMovieRepository Movies { get; }
        public IUserRepository Users { get; }
        public IGenericRepository<Rental> Rentals { get; }
        public IGenericRepository<Review> Reviews { get; }
        public IGenericRepository<RentalPricing> RentalPricings { get; }
        public IGenericRepository<NotificationLog> NotificationLogs { get; }

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