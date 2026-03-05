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
            var data = await cache.GetStringAsync(key);
            return data == null ? default : JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
            };
            await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
        }

        public async Task RemoveAsync(string key) => await cache.RemoveAsync(key);

        public async Task RemoveByPrefixAsync(string prefix)
        {
            
            await Task.CompletedTask;                                                    
        }
    }
}