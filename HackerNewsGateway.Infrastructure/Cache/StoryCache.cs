using System.Collections.Immutable;
using HackerNewsGateway.Domain.Entities;
using HackerNewsGateway.Domain.Interfaces;

namespace HackerNewsGateway.Infrastructure.Cache;

public sealed class StoryCache : IStoryCache
{
    private ImmutableList<Story> _stories = ImmutableList<Story>.Empty;

    public bool IsEmpty => _stories.IsEmpty;

    public void Replace(IEnumerable<Story> stories) =>
        Interlocked.Exchange(ref _stories, stories.ToImmutableList());

    public IEnumerable<Story> Take(int n) =>
        Volatile.Read(ref _stories).Take(n);
}
