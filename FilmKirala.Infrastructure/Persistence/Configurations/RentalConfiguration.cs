using FilmKirala.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FilmKirala.Infrastructure.Persistence.Configurations
{
    public class RentalConfiguration : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            builder.HasKey(r => r.Id);

            // 1. User -> Rental İlişkisi
            builder.HasOne(r => r.User)
                   .WithMany()
                   .HasForeignKey("UserId") // <--- Shadow Property
                   .OnDelete(DeleteBehavior.Cascade);

            // 2. Movie -> Rental İlişkisi (Restrict: Film silinirse faturalar patlamasın)
            builder.HasOne(r => r.Movie)
                   .WithMany()
                   .HasForeignKey("MovieId") // <--- Shadow Property
                   .OnDelete(DeleteBehavior.Restrict);

            // 3. RentalPricing -> Rental İlişkisi (Fiyat tarifesi silinirse geçmiş bozulmasın)
            builder.HasOne(r => r.RentalPricing)
                   .WithMany()
                   .HasForeignKey("RentalPricingId") // <--- Shadow Property
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}