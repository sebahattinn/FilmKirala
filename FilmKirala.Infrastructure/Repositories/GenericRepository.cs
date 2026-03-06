using System.Linq.Expressions;
using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FilmKirala.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.AsNoTracking().ToListAsync();

        public async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query
                        .OrderByDescending(x => EF.Property<int>(x, "Id"))
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
        }

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public void Remove(T entity) => _dbSet.Remove(entity);
        public void Update(T entity) => _dbSet.Update(entity);

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();
    }
}