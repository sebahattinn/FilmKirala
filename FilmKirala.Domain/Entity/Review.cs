using System;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Domain.Entity
{
    public class Review
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int MovieId { get; private set; }

        public User? User { get; private set; }    
        public Movie? Movie { get; private set; }


        public Rating Rating { get; private set; }
        public string Comment { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }

        private Review() { }  

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