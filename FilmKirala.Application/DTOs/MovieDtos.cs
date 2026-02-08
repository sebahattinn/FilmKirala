using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.DTOs
{
    // --- 1. YAZMA İŞLEMLERİ (Command) ---

    public record CreateMovieDto
    {
        public string Title { get; init; }
        public string Description { get; init; }
        public string Genre { get; init; }
        public int Stock { get; init; }
        // Liste boş gelirse patlamasın diye new() ile başlatıyoruz
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

    // Yardımcı parça
    public record PricingDto
    {
        public DurationType DurationType { get; init; }
        public int DurationValue { get; init; }
        public int Price { get; init; }
    }


    // --- 2. OKUMA İŞLEMLERİ (Query) ---

    public record MovieListDto
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public string Genre { get; init; }
        public bool IsStockAvailable { get; init; }
        public decimal MinPrice { get; init; }
    }

    // Detay: Her şey var.
    public record MovieDetailDto
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public string Genre { get; init; }
        public int Stock { get; init; }
        public bool IsActive { get; init; }

        // Null Reference yememek için listeleri initialize ediyoruz
        public List<PricingDto> RentalOptions { get; init; } = new();
        public List<ReviewDto> Reviews { get; init; } = new();

        public double AverageRating { get; init; }
    }
}