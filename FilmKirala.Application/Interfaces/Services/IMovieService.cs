using FilmKirala.Application.DTOs;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IMovieService
    {
        //sayfalamayı büyük veri kullanmaya çalıştığım için şey ettm
        Task<IEnumerable<MovieListDto>> GetAllMoviesAsync(int page = 1, int pageSize = 20);
        Task<MovieDetailDto?> GetMovieByIdAsync(int id); //filmlerin fiuat  ve puanlarını da getirebilmek için lazım olur
        Task AddMovieAsync(CreateMovieDto createMovieDto);
        Task AddRentalPricingAsync(int movieId, DurationType durationType, int price);
    }
}