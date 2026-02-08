namespace FilmKirala.Application.DTOs
{
    // Kiralama İsteği
    public record RentRequestDto(int MovieId, int PricingId);

    // Kiralamalarım Listesi (Constructor hatasını gidermek için {} kullandık)
    public record RentalListDto
    {
        public int Id { get; init; }
        public string MovieTitle { get; set; } // Set edilebilir yaptık ki Service'de doldurabilelim
        public DateTime StartRentalDate { get; init; } // Veritabanındaki isme uyumlu olsun
        public DateTime EndRentalDate { get; init; }
        public int TotalPrice { get; init; }
        public bool Status { get; init; }

        // Bu alanı hesaplatmak için getter kullanabiliriz
        public string RemainingTime => EndRentalDate > DateTime.UtcNow
            ? $"{(EndRentalDate - DateTime.UtcNow).Days} gün kaldı"
            : "Süresi doldu";
    }
}