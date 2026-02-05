namespace FilmKirala.Application.DTOs
{
    // Kiralama İsteği (Kullanıcı ID'si Token'dan alınacak, buraya yazılmaz)
    public record RentRequestDto(int MovieId, int PricingId);

    // "Kiralamalarım" sayfasında görünecek liste
    public record RentalListDto(
        int Id,
        string MovieTitle,
        DateTime StartDate,
        DateTime EndDate,
        int TotalPrice,
        bool IsActive, // Süresi dolmuş mu?
        string RemainingTime // Örn: "2 Gün Kaldı"
    );
}