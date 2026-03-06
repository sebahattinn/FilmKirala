using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
    }
}