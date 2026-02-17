using MessagePack;

namespace FilmKirala.Application.DTOs
{
    [MessagePackObject]
    public record CreateReviewDto
    {
        [Key(0)] public int MovieId { get; init; } //private set'ler uyarı verip duruyodu
        [Key(1)] public required string Comment { get; init; }
        [Key(2)] public int Rating { get; init; }
    }

    [MessagePackObject]
    public record ReviewDto
    {
        [Key(0)] public int Id { get; init; }
        [Key(1)] public required string Username { get; init; }
        [Key(2)] public required string Comment { get; init; }
        [Key(3)] public int Rating { get; init; }
        [Key(4)] public DateTime CreatedAt { get; init; }
    }
}