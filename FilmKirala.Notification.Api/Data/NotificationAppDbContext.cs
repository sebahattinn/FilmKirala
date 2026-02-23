using FilmKirala.Domain.Entity; // Ortak Domain sınıfımız
using Microsoft.EntityFrameworkCore;

namespace FilmKirala.Notification.Api.Data
{
    public class NotificationAppDbContext : DbContext
    {
        public NotificationAppDbContext(DbContextOptions<NotificationAppDbContext> options) : base(options) { }

        // Sadece bu tabloyla ilgileniyoruz
        public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    }
}