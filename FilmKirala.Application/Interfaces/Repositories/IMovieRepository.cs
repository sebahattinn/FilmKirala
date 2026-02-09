using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Interfaces.Repositories
{
    public interface IMovieRepository : IGenericRepository<Movie>
    {
        Task<Movie> GetMovieWithDetailsAsync(int id); //Fiyat seçeneği ve yorum da getirebilelim diye bunu yazdım
    }
}