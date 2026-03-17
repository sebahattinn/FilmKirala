using System.Collections.Concurrent;
using System.Text.Json;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace FilmKirala.Infrastructure.Services
{
    /// <summary>
    /// Two-level cache: L1 = IMemoryCache (in-process, always available),
    /// L2 = Redis (distributed, may be unavailable).
    ///
    /// GetOrSetAsync adds stampede protection: on cache miss, only one concurrent
    /// request executes the factory (DB query). All others wait and share the result.
    /// </summary>
    public class CacheService(IDistributedCache distributedCache, IMemoryCache memoryCache) : ICacheService
    {
        // L1 TTL: long enough that burst traffic never causes re-population.
        // Explicit RemoveAsync handles invalidation — TTL is just a safety net.
        private static readonly TimeSpan L1Duration = TimeSpan.FromMinutes(5);

        // One semaphore per cache key — only one DB call per key at a time.
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new();

        public async Task<T?> GetAsync<T>(string key)
        {
            // L1 — always succeeds, no network hop
            if (memoryCache.TryGetValue(key, out T? l1Value))
                return l1Value;

            // L2 — Redis may be unavailable; failure falls through silently
            try
            {
                var data = await distributedCache.GetStringAsync(key);
                if (data != null)
                {
                    var value = JsonSerializer.Deserialize<T>(data);
                    memoryCache.Set(key, value, L1Duration);
                    return value;
                }
            }
            catch (Exception)
            {
                // Redis is down — L1 already checked above, caller will hit the DB.
            }

            return default;
        }

        /// <summary>
        /// Cache-aside with per-key lock: prevents cache stampede under concurrent load.
        /// Pattern: Check → Lock → Double-check → DB → Write both levels.
        /// </summary>
        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            // Fast path — L1 hit (no locking needed)
            if (memoryCache.TryGetValue(key, out T? l1Value))
                return l1Value;

            // L2 hit — warm L1 and return without hitting DB
            try
            {
                var cached = await distributedCache.GetStringAsync(key);
                if (cached != null)
                {
                    var l2Value = JsonSerializer.Deserialize<T>(cached);
                    memoryCache.Set(key, l2Value, L1Duration);
                    return l2Value;
                }
            }
            catch (Exception) { /* Redis unavailable — continue to lock */ }

            // Cache miss — acquire a per-key lock so only one request hits the DB.
            // Timeout: if the lock is held longer than 8 seconds (DB under heavy load),
            // we fall through and execute the factory directly rather than keep the caller
            // waiting indefinitely. This caps worst-case latency.
            var semaphore = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            var acquired = await semaphore.WaitAsync(TimeSpan.FromSeconds(8));
            if (!acquired)
                return await factory();

            try
            {
                // Double-check: another thread may have populated the cache while we waited
                if (memoryCache.TryGetValue(key, out T? doubleCheck))
                    return doubleCheck;

                // Only one request reaches here — execute the factory (DB query)
                var result = await factory();
                if (result is not null)
                    await SetAsync(key, result, expiration);

                return result;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            // L1 — always write, never throws
            memoryCache.Set(key, value, L1Duration);

            // L2 — best effort; Redis failure does not break the request
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
                };
                await distributedCache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
            }
            catch (Exception)
            {
                // Redis unavailable — L1 is already set, DB is protected for L1Duration.
            }
        }

        public async Task RemoveAsync(string key)
        {
            // Remove from both levels so stale data never survives in L1 after invalidation
            memoryCache.Remove(key);

            try
            {
                await distributedCache.RemoveAsync(key);
            }
            catch (Exception)
            {
                // Redis unavailable — L1 is cleared, next miss will refresh from DB.
            }
        }

        public Task RemoveByPrefixAsync(string prefix)
        {
            // IMemoryCache has no built-in prefix scan; Redis-side prefix removal
            // requires SCAN which is intentionally omitted for simplicity.
            return Task.CompletedTask;
        }
    }
}
