using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Domain.Entity;
using FilmKirala.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FilmKirala.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

       
        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens) // Token listesini de çekmezsen patlarız
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));
        }
    }
}