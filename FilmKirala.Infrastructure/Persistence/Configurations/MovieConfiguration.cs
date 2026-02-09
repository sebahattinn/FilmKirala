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

          
            builder.HasMany(m => m.RentalPricings)
                   .WithOne(rp => rp.Movie)
                   .HasForeignKey("MovieId")
                   .OnDelete(DeleteBehavior.Cascade);

              //özel ayarlar bir filmin birden fazla yorumu olur, bir yorumun tek filmi olur ve en son da film silinirse yorumlarda silinir.
            builder.HasMany(m => m.Reviews)       
                   .WithOne(r => r.Movie)        
                   .HasForeignKey(r => r.MovieId) 
                   .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}