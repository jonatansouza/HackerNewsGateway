using HackerNewsGateway.Domain.Entities;

namespace HackerNewsGatewayApi.Cache;

public interface IStoryCache
{
    bool IsEmpty { get; }
    void Replace(IEnumerable<Story> stories);
    IEnumerable<Story> Take(int n);
}
