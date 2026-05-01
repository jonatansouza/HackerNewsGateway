using HackerNewsGateway.Domain.Entities;

namespace HackerNewsGateway.Domain.Services;

public static class StoryRanking
{
    public static IReadOnlyList<Story> FromResults(IEnumerable<Story?> results) =>
        results
            .Where(s => s is not null)
            .OrderByDescending(s => s!.Score)
            .Select(s => s!)
            .ToList();
}
