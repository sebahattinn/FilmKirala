using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.DTOs
{
    public record CreateMovieDto
    {
        public string Title { get; init; }
        public string Description { get; init; }
        public string Genre { get; init; }
        public int Stock { get; init; }
        // Liste boş gelirse patlamasın diye new kullandım
        public List<PricingDto> Pricings { get; init; } = new();
    }

    public record UpdateMovieDto
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public string Genre { get; init; }
        public int Stock { get; init; }
        public bool IsActive { get; init; }
    }

    public record PricingDto
    {
        public DurationType DurationType { get; init; }
        public int DurationValue { get; init; }
        public int Price { get; init; }
    }

    public record MovieListDto
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public string Genre { get; init; }
        public bool IsStockAvailable { get; init; }
        public decimal MinPrice { get; init; }
    }

    public record MovieDetailDto
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public string Genre { get; init; }
        public int Stock { get; init; }
        public bool IsActive { get; init; }
        public List<PricingDto> RentalOptions { get; init; } = new();
        public List<ReviewDto> Reviews { get; init; } = new();

        public double AverageRating { get; init; }
    }
}