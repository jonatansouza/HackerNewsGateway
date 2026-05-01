using HackerNewsGateway.Domain.Options;
using HackerNewsGateway.Infrastructure.Http;
using HackerNewsGatewayApi.Cache;
using Microsoft.Extensions.Options;

namespace HackerNewsGatewayApi.Workers;

public sealed class StorySyncWorker(
    HackerNewsClient hackerNewsClient,
    IStoryCache cache,
    IOptions<HackerNewsOptions> options,
    ILogger<StorySyncWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var interval = TimeSpan.FromMinutes(options.Value.SyncIntervalMinutes);
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
            var semaphore = new SemaphoreSlim(options.Value.MaxParallelRequests, options.Value.MaxParallelRequests);

            var tasks = ids.Select(async id =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    return await hackerNewsClient.GetStoryAsync(id, ct);
                }
                finally
                {
                    semaphore.Release();
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
