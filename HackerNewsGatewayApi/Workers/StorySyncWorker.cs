using HackerNewsGateway.Infrastructure.Http;
using HackerNewsGatewayApi.Cache;

namespace HackerNewsGatewayApi.Workers;

public sealed class StorySyncWorker(
    HackerNewsClient hackerNewsClient,
    IStoryCache cache,
    IConfiguration configuration,
    ILogger<StorySyncWorker> logger) : BackgroundService
{
    private readonly SemaphoreSlim _semaphore = new(20, 20);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var interval = TimeSpan.FromMinutes(
            configuration.GetValue<int>("HackerNews:SyncIntervalMinutes", 5));

        using var timer = new PeriodicTimer(interval);

        do
        {
            await SyncAsync(ct);
        }
        while (await timer.WaitForNextTickAsync(ct));
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Starting story sync.");

            var ids = await hackerNewsClient.GetBestStoryIdsAsync(ct);

            var tasks = ids.Select(async id =>
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    return await hackerNewsClient.GetStoryAsync(id, ct);
                }
                finally
                {
                    _semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);

            var sorted = results
                .Where(s => s is not null)
                .OrderByDescending(s => s!.Score)
                .Select(s => s!)
                .ToList();

            cache.Replace(sorted);

            logger.LogInformation("Story sync completed. {Count} stories cached.", sorted.Count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Story sync failed. Serving stale cache.");
        }
    }
}
