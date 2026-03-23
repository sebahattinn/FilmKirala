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

            builder.HasOne(r => r.User)
                   .WithMany()
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Movie)
                   .WithMany()
                   .HasForeignKey(r => r.MovieId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.RentalPricing)
                   .WithMany()
                   .HasForeignKey(r => r.RentalPricingId)
                   .OnDelete(DeleteBehavior.Restrict);

            // GROUP BY MovieId in ReportService and expiration worker filter
            builder.HasIndex(r => r.MovieId)
                   .HasDatabaseName("IX_Rentals_MovieId");

            // Expiration worker: WHERE Status = 1 AND EndRentalDate <= now
            builder.HasIndex(r => new { r.Status, r.EndRentalDate })
                   .HasDatabaseName("IX_Rentals_Status_EndRentalDate");
        }
    }
}