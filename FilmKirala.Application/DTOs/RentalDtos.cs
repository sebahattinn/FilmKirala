namespace FilmKirala.Application.DTOs
{
    public record RentRequestDto(int MovieId, int PricingId);

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