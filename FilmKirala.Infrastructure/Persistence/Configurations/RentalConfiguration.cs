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
                   .HasForeignKey(r => r.UserId) // ✅ String yerine Lambda
                   .OnDelete(DeleteBehavior.Cascade);

            // 2. Movie -> Rental İlişkisi
            builder.HasOne(r => r.Movie)
                   .WithMany()
                   .HasForeignKey(r => r.MovieId) // ✅ String yerine Lambda
                   .OnDelete(DeleteBehavior.Restrict);

            // 3. RentalPricing -> Rental İlişkisi
            builder.HasOne(r => r.RentalPricing)
                   .WithMany()
                   .HasForeignKey(r => r.RentalPricingId) // ✅ String yerine Lambda
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}