using HackerNewsGateway.Domain.Entities;
using HackerNewsGateway.Domain.Interfaces;
using HackerNewsGateway.Domain.Options;
using HackerNewsGateway.Domain.Services;
using HackerNewsGateway.Infrastructure.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HackerNewsGateway.Infrastructure.Workers;

public sealed class StorySyncWorker(
    HackerNewsClient hackerNewsClient,
    IStoryCache cache,
    IOptions<HackerNewsOptions> options,
    ILogger<StorySyncWorker> logger) : BackgroundService
{
    private readonly SemaphoreSlim _syncGuard = new(1, 1);

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
        // skip if previous sync is still running
        if (!await _syncGuard.WaitAsync(0, ct))
        {
            logger.LogWarning("Sync skipped — previous sync still in progress.");
            return;
        }

        try
        {
            logger.LogInformation("Starting story sync.");

            var ids = await hackerNewsClient.GetBestStoryIdsAsync(ct);
            var allResults = new List<Story?>();

            foreach (var batch in ids.Chunk(options.Value.SyncBatchSize))
            {
                var batchResults = await Task.WhenAll(
                    batch.Select(id => hackerNewsClient.GetStoryAsync(id, ct)));

                allResults.AddRange(batchResults);
            }

            var ranked = StoryRanking.FromResults(allResults);
            cache.Replace(ranked);

            logger.LogInformation("Story sync completed. {Count} stories cached.", ranked.Count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Story sync failed. Serving stale cache.");
        }
        finally
        {
            _syncGuard.Release();
        }
    }
}
