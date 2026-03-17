using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.DTOs
{
    public record RentRequestDto(int MovieId, DurationType DurationType, int Quantity = 1);

    public record RentResponseDto(
        bool Success,
        string Message,
        int TotalCost, 
        int RemainingBalance, 
        DateTime RentalEndDate,
        string RentalType,
        int RentalDuration,
        string Summary
    );
    public record RentalListDto
    {
        public int Id { get; init; }
        public int MovieId { get; init; }
        public string MovieTitle { get; init; } = null!;
        public DateTime StartRentalDate { get; init; }
        public DateTime EndRentalDate { get; init; }
        public int TotalPrice { get; init; } 
        public bool Status { get; init; }
        public string RemainingTime
        {
            get
            {
                var remaining = EndRentalDate - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero) return "Süresi doldu";
                if (remaining.TotalDays >= 7)    return $"{(int)(remaining.TotalDays / 7)} hafta kaldı";
                if (remaining.TotalHours >= 24)  return $"{(int)remaining.TotalDays} gün kaldı";
                if (remaining.TotalMinutes >= 60) return $"{(int)remaining.TotalHours} saat kaldı";
                return $"{(int)remaining.TotalMinutes} dakika kaldı";
            }
        }
    }
}