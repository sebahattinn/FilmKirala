using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IMovieService
    {
        // Vitrin için tüm filmleri getirir
        Task<IEnumerable<MovieListDto>> GetAllMoviesAsync();

        // Filmin detayını (Fiyatlar, Puanlar) getirir
        Task<MovieDetailDto> GetMovieByIdAsync(int id);

        // Yeni film ekler (DTO içinde fiyatlar da olacak)
        Task AddMovieAsync(CreateMovieDto createMovieDto);

        // Stok kontrolü vb. buradaki metodlar içinde dönecek
    }
}