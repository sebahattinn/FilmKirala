using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IRentalService
    {
        Task<RentResponseDto> RentMovieAsync(RentRequestDto request, int userId);
        Task<IEnumerable<RentalListDto>> GetUserRentalsAsync(int userId);
        Task CheckExpiredRentalsAsync();
    }
}