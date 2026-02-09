using FilmKirala.Application.DTOs;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IMovieService
    {
        Task<IEnumerable<MovieListDto>> GetAllMoviesAsync();
        Task<MovieDetailDto?> GetMovieByIdAsync(int id); //filmlerin fiuat  ve puanlarını da getirebilmek için lazım olur
        Task AddMovieAsync(CreateMovieDto createMovieDto);
        Task AddRentalPricingAsync(int movieId, DurationType durationType, int price);
    }
}