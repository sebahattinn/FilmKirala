using FilmKirala.Domain.Enums;
using MessagePack;

namespace FilmKirala.Application.DTOs
{
    [MessagePackObject]
    public record CreateMovieDto
    {
        [Key(0)] public required string Title { get; init; }
        [Key(1)] public required string Description { get; init; }
        [Key(2)] public required string Genre { get; init; }
        [Key(3)] public int Stock { get; init; }

        [Key(4)] public List<PricingDto> Pricings { get; set; } = [];
    }

    [MessagePackObject]
    public record UpdateMovieDto
    {
        [Key(0)] public int Id { get; init; }
        [Key(1)] public required string Title { get; init; }
        [Key(2)] public required string Description { get; init; }
        [Key(3)] public required string Genre { get; init; }
        [Key(4)] public int Stock { get; init; }
        [Key(5)] public bool IsActive { get; init; }
    }

    [MessagePackObject]
    public record PricingDto
    {
        [Key(0)] public DurationType DurationType { get; init; }
        [Key(1)] public int DurationValue { get; init; }
        [Key(2)] public int Price { get; init; }
    }

    [MessagePackObject]
    public record MovieListDto
    {
        [Key(0)] public int Id { get; init; }
        [Key(1)] public required string Title { get; init; }
        [Key(2)] public required string Genre { get; init; }
        [Key(3)] public bool IsStockAvailable { get; init; }
        [Key(4)] public double MinPrice { get; init; }
    }

    [MessagePackObject]
    public record MovieDetailDto
    {
        [Key(0)] public int Id { get; init; }
        [Key(1)] public required string Title { get; init; }
        [Key(2)] public required string Description { get; init; }
        [Key(3)] public required string Genre { get; init; }
        [Key(4)] public int Stock { get; init; }
        [Key(5)] public bool IsActive { get; init; }

        [Key(6)] public List<PricingDto> RentalOptions { get; set; } = [];
        [Key(7)] public List<ReviewDto> Reviews { get; set; } = [];
        [Key(8)] public double AverageRating { get; set; }
    }
}