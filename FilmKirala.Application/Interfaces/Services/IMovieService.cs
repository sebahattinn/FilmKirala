using FilmKirala.Application.DTOs;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IMovieService
    {
        Task<IEnumerable<MovieListDto>> GetAllMoviesAsync(string? search = null, string? genre = null, int page = 1, int pageSize = 20);
        Task<MovieDetailDto?> GetMovieByIdAsync(int id);
        Task CreateMovieAsync(CreateMovieDto createMovieDto);
        Task AddRentalPricingAsync(int movieId, DurationType durationType, int price);
    }
}