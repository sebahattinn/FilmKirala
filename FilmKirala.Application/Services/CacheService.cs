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
                // Redis kapalı olsa bile null dönerek sistemin DB'den devam etmesini sağlarız.
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
                // Redis bağlantı hatası durumunda işlemi kesmiyoruz.
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
                // Key silinemese bile ana işlemin (kiralama vb.) devam etmesi sağlanır.
            }
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            await Task.CompletedTask;
        }
    }
}