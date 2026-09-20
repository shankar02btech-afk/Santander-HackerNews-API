using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsApi.Caching;

public class StoryCacheService : IStoryCacheService
{
    private readonly IMemoryCache _cache;

    public StoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);

        return Task.FromResult(value);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration)
    {
        _cache.Set(key, value, expiration);

        return Task.CompletedTask;
    }
}