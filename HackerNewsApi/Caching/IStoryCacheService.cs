namespace HackerNewsApi.Caching;

public interface IStoryCacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration);
}