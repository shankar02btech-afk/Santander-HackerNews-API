using System.Net.Http.Json;
using HackerNewsApi.Caching;
using HackerNewsApi.Models;

namespace HackerNewsApi.Services;

public class HackerNewsService : IHackerNewsService
{
    private const string BestStoriesCacheKey = "hackernews:best-story-ids";

    private static readonly TimeSpan CacheDuration =
        TimeSpan.FromMinutes(5);

    private readonly HttpClient _httpClient;
    private readonly IStoryCacheService _cache;
    private readonly ILogger<HackerNewsService> _logger;

    // Prevent too many simultaneous calls to Hacker News.
    private readonly SemaphoreSlim _semaphore = new(10, 10);

    public HackerNewsService(
        HttpClient httpClient,
        IStoryCacheService cache,
        ILogger<HackerNewsService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StoryResponse>> GetBestStoriesAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var storyIds =
            await GetBestStoryIdsAsync(cancellationToken);

        /*
         * Hacker News beststories.json is already a ranking of IDs,
         * but we must determine the ranking by score.
         *
         * We fetch a bounded set of candidates and sort them by score.
         */
        var candidateIds = storyIds.ToList();

        var tasks = candidateIds.Select(
            id => GetStoryAsync(id, cancellationToken));

        var stories = await Task.WhenAll(tasks);

        return stories
            .Where(story => story is not null)
            .Select(story => story!)
            .OrderByDescending(story => story.Score)
            .Take(count)
            .ToList();
    }

    private async Task<List<int>> GetBestStoryIdsAsync(
        CancellationToken cancellationToken)
    {
        var cached =
            await _cache.GetAsync<List<int>>(BestStoriesCacheKey);

        if (cached is not null)
        {
            return cached;
        }

        _logger.LogInformation(
            "Fetching best story IDs from Hacker News.");

        var ids = await _httpClient.GetFromJsonAsync<List<int>>(
            "beststories.json",
            cancellationToken);

        ids ??= new List<int>();

        await _cache.SetAsync(
            BestStoriesCacheKey,
            ids,
            CacheDuration);

        return ids;
    }

    private async Task<StoryResponse?> GetStoryAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"hackernews:story:{id}";

        var cached =
            await _cache.GetAsync<StoryResponse>(cacheKey);

        if (cached is not null)
        {
            return cached;
        }

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            // Double-check cache after waiting for the semaphore.
            cached =
                await _cache.GetAsync<StoryResponse>(cacheKey);

            if (cached is not null)
            {
                return cached;
            }

            var item = await _httpClient.GetFromJsonAsync<HackerNewsItem>(
                $"item/{id}.json",
                cancellationToken);

            if (item is null)
            {
                return null;
            }

            var story = new StoryResponse
            {
                Title = item.Title ?? string.Empty,
                Uri = item.Url ?? string.Empty,
                PostedBy = item.By ?? string.Empty,
                Time = DateTimeOffset.FromUnixTimeSeconds(item.Time),
                Score = item.Score,
                CommentCount = item.Descendants
            };

            await _cache.SetAsync(
                cacheKey,
                story,
                CacheDuration);

            return story;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve Hacker News story {StoryId}",
                id);

            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}