using System; 
using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.DTOs
{
    public record CreateReviewDto
    {
        public int MovieId { get; init; }
        public string Comment { get; init; }
        public Rating Rating { get; init; }
    }
    public record ReviewDto
    {
        public int Id { get; init; }
        public string Username { get; init; } 
        public string Comment { get; init; }
        public int Rating { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}