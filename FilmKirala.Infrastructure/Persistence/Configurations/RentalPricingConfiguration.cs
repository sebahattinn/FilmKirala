using FilmKirala.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FilmKirala.Infrastructure.Persistence.Configurations
{
    public class RentalPricingConfiguration : IEntityTypeConfiguration<RentalPricing>
    {
        public void Configure(EntityTypeBuilder<RentalPricing> builder)
        {
            builder.HasKey(rp => rp.Id);

            builder.Property(rp => rp.Price).IsRequired().HasPrecision(18, 2);

            // Aynı filme aynı türde iki fiyat paketi eklenemez
            builder.HasIndex(rp => new { rp.MovieId, rp.DurationType }).IsUnique();
        }
    }
}