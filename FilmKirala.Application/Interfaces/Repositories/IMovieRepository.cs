using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Interfaces.Repositories
{
    public interface IMovieRepository : IGenericRepository<Movie>
    {
        // Filmi detaylarıyla (Fiyat seçenekleri ve Yorumlar) getirir
        Task<Movie> GetMovieWithDetailsAsync(int id);
    }
}