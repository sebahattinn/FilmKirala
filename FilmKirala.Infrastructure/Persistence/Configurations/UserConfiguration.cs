using FilmKirala.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary Key
        builder.HasKey(u => u.Id);

        // Properties
        builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

        builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Roles).IsRequired();

        // 👇 YENİ EKLENEN AYARLAR 👇
        // Refresh Token null olabilir (ilk kayıtta oluşmayabilir veya çıkış yapınca silinebilir)
        builder.Property(u => u.RefreshToken)
               .IsRequired(false)
               .HasMaxLength(200); // 200 karakter yeterli

        builder.Property(u => u.RefreshTokenExpiryTime)
               .IsRequired(false);
    }
}