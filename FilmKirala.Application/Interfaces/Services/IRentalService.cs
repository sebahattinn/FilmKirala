using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IRentalService
    {
        Task<RentResponseDto> CreateRentalAsync(RentRequestDto request, int userId);
        Task<IEnumerable<RentalListDto>> GetUserRentalsAsync(int userId);
        Task CheckExpiredRentalsAsync();
    }
}