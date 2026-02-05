using FilmKirala.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FilmKirala.Infrastructure.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Comment).HasMaxLength(500);

            // User -> Review
            builder.HasOne(r => r.User)
                   .WithMany()
                   .HasForeignKey(r => r.UserId) // Tırnak yok, gerçek property var!
                   .OnDelete(DeleteBehavior.Cascade);

            // Movie -> Review
            builder.HasOne(r => r.Movie)
                   .WithMany()
                   .HasForeignKey(r => r.MovieId) // Tırnak yok, gerçek property var!
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}