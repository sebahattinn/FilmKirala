using System.Reflection;
using FilmKirala.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace FilmKirala.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Rental> Rentals => Set<Rental>();
        public DbSet<RentalPricing> RentalPricings => Set<RentalPricing>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
        public DbSet<NotificationLog> NotificationLogs => Set< NotificationLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);  //  Tüm konfigürasyonları otomatik olarak assembly'den oku
          //  modelBuilder.Entity<NotificationLog>().Ignore(c => c.Type); bunu iptal edip yerine entities'e atribute koyuyorum.
        }
    }
}