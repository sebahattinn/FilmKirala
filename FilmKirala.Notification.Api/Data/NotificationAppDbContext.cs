using FilmKirala.Domain.Entity; 
using Microsoft.EntityFrameworkCore;

namespace FilmKirala.Notification.Api.Data
{
    public class NotificationAppDbContext : DbContext
    {
        public NotificationAppDbContext(DbContextOptions<NotificationAppDbContext> options) : base(options) { }

      
        public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    }
}