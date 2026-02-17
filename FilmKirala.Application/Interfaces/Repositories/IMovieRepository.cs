using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Interfaces.Repositories
{
    public interface IMovieRepository : IGenericRepository<Movie>
    {
        Task<Movie?> GetMovieWithDetailsAsync(int id); //fiyat seçeneği ve yorum için.
    }
}