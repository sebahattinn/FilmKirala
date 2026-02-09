using FilmKirala.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RentalPricingConfiguration : IEntityTypeConfiguration<RentalPricing>
{
    public void Configure(EntityTypeBuilder<RentalPricing> builder)
    {
        builder.HasKey(rp => rp.Id);

        // Eğer property int ise veritabanında int tutulur. //decimal'e çevrlilecekse .HasPrecision(18,2) eklemek lazım olur.
        builder.Property(rp => rp.Price).IsRequired();
    }
}