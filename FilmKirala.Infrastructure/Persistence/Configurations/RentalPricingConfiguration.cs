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
            //  Para birimi olduğu için 18 basamak, 2 kuruş hanesi zorunlu
        }
    }
}