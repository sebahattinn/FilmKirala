using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FilmKirala.Notification.Api.Data
{
    public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationAppDbContext>
    {
        public NotificationAppDbContext CreateDbContext(string[] args)
        {
            // Appsettings.json dosyasını bulup okuyoruz
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            //  DbContext ayarlarını manuel olarak oluşturuyoruz
            var builder = new DbContextOptionsBuilder<NotificationAppDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString);

            //  Hazır ayarlarla Context'i geri döndürüyoruz
            return new NotificationAppDbContext(builder.Options);
        }
    }
}