using System.Text.Json;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace FilmKirala.Infrastructure.Services
{
    public class CacheService(IDistributedCache cache) : ICacheService
    {
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var data = await cache.GetStringAsync(key);
                return data == null ? default : JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception)
            {
                // Even if Redis is down, we ensure the system continues from the database by returning null.
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
                };
                await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
            }
            catch (Exception)
            {
                // We do not terminate the process in case of a Redis connection error.
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await cache.RemoveAsync(key);
            }
            catch (Exception)
            {
                // Even if the key cannot be deleted, the main process (rental, etc.) continues.
            }
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            await Task.CompletedTask;
        }
    }
}