using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IRentalService
    {
       
        Task<RentResponseDto> RentMovieAsync(RentRequestDto request, int userId); // UserId token'dan gelecek ve kullanıcının film kiralayabilmesi için tüm kontrolleri burası yapacak

        Task<IEnumerable<RentalListDto>> GetUserRentalsAsync(int userId);        //kiraladığı listeler 
        Task CheckExpiredRentalsAsync();   //süre dolunca rent olayını pasife çekmek için check edioyz
    }
}