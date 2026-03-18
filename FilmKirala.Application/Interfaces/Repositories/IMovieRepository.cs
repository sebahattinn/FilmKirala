using FilmKirala.Application.DTOs;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Interfaces.Repositories
{
    public interface IMovieRepository : IGenericRepository<Movie>
    {
        Task<MovieDetailDto?> GetMovieWithDetailsAsync(int id);
    }
}