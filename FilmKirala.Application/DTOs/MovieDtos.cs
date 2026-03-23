using FilmKirala.Domain.Enums;
using MessagePack;

namespace FilmKirala.Application.DTOs
{
    /// <summary>
    /// Yeni film oluşturma isteği.
    /// </summary>
    [MessagePackObject]
    public record CreateMovieDto
    {
        /// <summary>Film adı. Zorunlu, maksimum 300 karakter.</summary>
        [Key(0)] public required string Title { get; init; }

        /// <summary>Film açıklaması. Zorunlu, en az 5 karakter.</summary>
        [Key(1)] public required string Description { get; init; }

        /// <summary>Film türü. Örnek: "Aksiyon", "Drama", "Komedi".</summary>
        [Key(2)] public required string Genre { get; init; }

        /// <summary>Mevcut stok adedi. 0 veya daha büyük olmalıdır.</summary>
        [Key(3)] public int Stock { get; init; }

        /// <summary>
        /// Kiralama seçenekleri. İsteğe bağlıdır, boş bırakılabilir.
        /// Her bir seçenek farklı bir süre tipi (saatlik, günlük vb.) ve fiyat tanımlar.
        /// </summary>
        [Key(4)] public List<CreatePricingDto> Pricings { get; set; } = [];
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

    /// <summary>
    /// Film oluştururken gönderilen kiralama seçeneği (POST /api/Movies body'sinde kullanılır).
    /// </summary>
    [MessagePackObject]
    public record CreatePricingDto
    {
        /// <summary>
        /// Kiralama süresinin birimi.
        /// 1 = Saatlik, 2 = Günlük, 3 = Haftalık, 4 = Aylık, 5 = Yıllık.
        /// </summary>
        [Key(0)] public DurationType DurationType { get; init; }

        /// <summary>
        /// Kiralama süresi değeri. DurationType ile birlikte yorumlanır.
        /// Örnek: DurationType=2 (Günlük) ve DurationValue=3 → 3 günlük kiralama.
        /// </summary>
        [Key(1)] public int DurationValue { get; init; }

        /// <summary>Bu seçenek için fiyat (TL cinsinden). 0'dan büyük olmalıdır.</summary>
        [Key(2)] public int Price { get; init; }
    }

    /// <summary>
    /// Mevcut bir kiralama seçeneğini temsil eder (okuma ve güncelleme işlemlerinde kullanılır).
    /// </summary>
    [MessagePackObject]
    public record PricingDto
    {
        /// <summary>
        /// Kiralama süresinin birimi.
        /// 1 = Saatlik, 2 = Günlük, 3 = Haftalık, 4 = Aylık, 5 = Yıllık.
        /// </summary>
        [Key(0)] public DurationType DurationType { get; init; }

        /// <summary>
        /// Kiralama süresi değeri. DurationType ile birlikte yorumlanır.
        /// Örnek: DurationType=2 (Günlük) ve DurationValue=3 → 3 günlük kiralama.
        /// </summary>
        [Key(1)] public int DurationValue { get; init; }

        /// <summary>Bu seçenek için fiyat (TL cinsinden). 0'dan büyük olmalıdır.</summary>
        [Key(2)] public int Price { get; init; }

        /// <summary>Fiyat kaydının veritabanı ID'si (güncelleme işlemlerinde kullanılır).</summary>
        [Key(3)] public int Id { get; init; }
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