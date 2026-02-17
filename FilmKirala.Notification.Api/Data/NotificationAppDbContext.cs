using FilmKirala.Notification.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FilmKirala.Notification.Api.Data
{
    public class NotificationAppDbContext : DbContext
    {
        public NotificationAppDbContext(DbContextOptions<NotificationAppDbContext> options) : base(options)
        {
        }

        public DbSet<NotificationLog> NotificationLogs { get; set; }
    }
}