using System;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Domain.Entity
{
    public class Review
    {
        public int Id { get; private set; }

        // --- ID'leri geri getirdik (En sağlam yöntem) ---
        public int UserId { get; private set; }
        public int MovieId { get; private set; }

        // --- Navigation Propertyler ---
        public User? User { get; private set; }
        public Movie? Movie { get; private set; }

        // Bu opsiyonel, null olabilir
        public int? RentalPricingId { get; private set; }
        public RentalPricing? RentalPricing { get; private set; }

        public Rating Rating { get; private set; }
        public string Comment { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // EF Core'un veritabanından okurken kullandığı boş constructor
        // BU ÇOK ÖNEMLİ, SİLME!
        private Review() { }

        // Yeni kayıt atarken kullanacağın constructor
        public Review(int userId, int movieId, string comment, Rating rating)
        {
            if (userId <= 0) throw new ArgumentException("Geçersiz User Id");
            if (movieId <= 0) throw new ArgumentException("Geçersiz Movie Id");
            if (string.IsNullOrWhiteSpace(comment)) throw new ArgumentException("Yorum boş olamaz");

            UserId = userId;
            MovieId = movieId;
            Comment = comment;
            Rating = rating;
            CreatedAt = DateTime.UtcNow;
        }
    }
}