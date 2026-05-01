using System.Net.Http.Json;
using HackerNewsGateway.Domain.Entities;
using HackerNewsGateway.Infrastructure.Dtos;

namespace HackerNewsGateway.Infrastructure.Http;

public sealed class HackerNewsClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken ct = default)
    {
        var ids = await httpClient.GetFromJsonAsync<int[]>("/v0/beststories.json", ct);
        return ids ?? [];
    }

    public async Task<Story?> GetStoryAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var dto = await httpClient.GetFromJsonAsync<HackerNewsStoryDto>($"/v0/item/{id}.json", ct);

            if (dto is null || dto.Title is null)
                return null;

            return new Story(
                Title: dto.Title,
                Uri: dto.Url ?? string.Empty,
                PostedBy: dto.By ?? string.Empty,
                Time: DateTimeOffset.FromUnixTimeSeconds(dto.Time),
                Score: dto.Score,
                CommentCount: dto.Descendants
            );
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
