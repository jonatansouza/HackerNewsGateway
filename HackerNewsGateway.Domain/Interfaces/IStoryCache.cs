using HackerNewsGateway.Domain.Entities;

namespace HackerNewsGateway.Domain.Interfaces;

public interface IStoryCache
{
    bool IsEmpty { get; }
    void Replace(IEnumerable<Story> stories);
    IEnumerable<Story> Take(int n);
}
