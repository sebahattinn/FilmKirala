using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.DTOs
{
    public record RentRequestDto(int MovieId, DurationType DurationType, int Quantity = 1);

    public record RentResponseDto(
        bool Success,
        string Message,
        int TotalCost, // int devam
        int RemainingBalance, // int devam
        DateTime RentalEndDate,
        string RentalType,
        int RentalDuration,
        string Summary
    );
    public record RentalListDto
    {
        public int Id { get; init; }
        public string MovieTitle { get; init; } = null!;
        public DateTime StartRentalDate { get; init; }
        public DateTime EndRentalDate { get; init; }
        public int TotalPrice { get; init; } // int devam
        public bool Status { get; init; }
        public string RemainingTime => EndRentalDate > DateTime.UtcNow
            ? $"{(EndRentalDate - DateTime.UtcNow).Days} gün kaldı"
            : "Süresi doldu";
    }
}