using System.Diagnostics.CodeAnalysis;

namespace HackerNewsGateway.Domain.ValueObjects;

public sealed class StoryCount
{
    public int Value { get; }

    private StoryCount(int value) => Value = value;

    public static StoryCountBuilder Config(int maxStories) => new(maxStories);

    public sealed class StoryCountBuilder
    {
        private readonly int _max;

        internal StoryCountBuilder(int max) => _max = max;

        public bool TryCreate(int value, [NotNullWhen(true)] out StoryCount? result, out string error)
        {
            if (value < 1)
            {
                result = null;
                error = "n must be greater than 0.";
                return false;
            }

            if (value > _max)
            {
                result = null;
                error = $"n must not exceed {_max}.";
                return false;
            }

            result = new StoryCount(value);
            error = string.Empty;
            return true;
        }
    }
}
