using FilmKirala.Domain.Enums; // DurationType için ekledim

namespace FilmKirala.Application.DTOs
{
    // Quantity (Adet/Süre) varsayılan 1 olarak eklendi
    // PricingId yerine DurationType geldi. Kullanıcı artık tip seçiyor (Saatlik, Günlük vb.)
    public record RentRequestDto(int MovieId, DurationType DurationType, int Quantity = 1);

    // Fiş kesmek için cevap DTO'su (Aynı dosyaya ekledim)
    public record RentResponseDto
    {
        public bool Success { get; init; }
        public string Message { get; init; }
        public int TotalCost { get; init; }
        public int RemainingBalance { get; init; }
        public DateTime RentalEndDate { get; init; }
        public string RentalType { get; init; }
        public int RentalDuration { get; init; }
        public string Summary { get; init; }
    }

    // Constructor hatasını gidermek için {} kullandım
    public record RentalListDto
    {
        public int Id { get; init; }
        public string MovieTitle { get; set; } // Set edilebilir yaptım  Service'de doldurabileyim diye
        public DateTime StartRentalDate { get; init; }
        public DateTime EndRentalDate { get; init; }
        public int TotalPrice { get; init; }
        public bool Status { get; init; }

        // Bu alanı hesaplamak için getter kullanabiliriz
        public string RemainingTime => EndRentalDate > DateTime.UtcNow
            ? $"{(EndRentalDate - DateTime.UtcNow).Days} gün kaldı"
            : "Süresi doldu";
    }
}