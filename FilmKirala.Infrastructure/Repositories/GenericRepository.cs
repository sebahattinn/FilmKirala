using System.Diagnostics;
using System.Linq.Expressions;
using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FilmKirala.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly ILogger<GenericRepository<T>> _logger; 

        public GenericRepository(AppDbContext context, ILogger<GenericRepository<T>> logger)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.AsNoTracking().ToListAsync();

        public async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null)
        {
            var sw = Stopwatch.StartNew();
            var entityName = typeof(T).Name;

            _logger.LogInformation("Pagination başladı: {EntityName}, Sayfa: {Page}, Boyut: {PageSize}", entityName, page, pageSize);

            try
            {
                IQueryable<T> query = _dbSet.AsNoTracking();

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                var result = await query
                            .OrderByDescending(x => EF.Property<object>(x, "Id")) 
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

                sw.Stop();
                _logger.LogInformation("Pagination başarıyla tamamlandı: {EntityName}. Süre: {Elapsed}ms, Kayıt Sayısı: {Count}",
                    entityName, sw.ElapsedMilliseconds, result.Count);

                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Pagination HATASI! {EntityName} tablosunda sorgu {Elapsed}ms sonra patladı.", entityName, sw.ElapsedMilliseconds);
                throw; 
            }
        }

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public void Remove(T entity) => _dbSet.Remove(entity);
        public void Update(T entity) => _dbSet.Update(entity);

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();
    }
}