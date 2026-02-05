using FilmKirala.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RentalPricingConfiguration : IEntityTypeConfiguration<RentalPricing>
{
    public void Configure(EntityTypeBuilder<RentalPricing> builder)
    {
        builder.HasKey(rp => rp.Id);

        // Eğer property int ise veritabanında int tutulur.
        // Eğer ileride decimal'a çevirirsen buraya .HasPrecision(18,2) eklemelisin.
        builder.Property(rp => rp.Price).IsRequired();
    }
}