using FilmKirala.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FilmKirala.Infrastructure.Persistence.Configurations
{
    public class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Title).IsRequired().HasMaxLength(300);
            builder.Property(m => m.Description).HasMaxLength(1000);

            // Movie -> RentalPricing İlişkisi
            // Bir filmin çokça fiyat seçeneği olabilir.
            builder.HasMany(m => m.RentalPricings)
                   .WithOne(rp => rp.Movie)
                   .HasForeignKey("MovieId") // <--- RentalPricing tablosunda gizli MovieId oluşur
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}