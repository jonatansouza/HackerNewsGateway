using HackerNewsGateway.Domain.ValueObjects;

namespace HackerNewsGateway.Tests;

public class StoryCountTests
{
    private static readonly int Max = 100;
    private static StoryCount.StoryCountBuilder Builder() => StoryCount.Config(Max);

    [Fact]
    public void TryCreate_Zero_ReturnsFalse()
    {
        Assert.False(Builder().TryCreate(0, out _, out var error));
        Assert.NotEmpty(error);
    }

    [Fact]
    public void TryCreate_Negative_ReturnsFalse()
    {
        Assert.False(Builder().TryCreate(-1, out _, out var error));
        Assert.NotEmpty(error);
    }

    [Fact]
    public void TryCreate_ExceedsMax_ReturnsFalse()
    {
        Assert.False(Builder().TryCreate(Max + 1, out _, out var error));
        Assert.NotEmpty(error);
    }

    [Fact]
    public void TryCreate_One_ReturnsTrue()
    {
        Assert.True(Builder().TryCreate(1, out var result, out _));
        Assert.Equal(1, result!.Value);
    }

    [Fact]
    public void TryCreate_Max_ReturnsTrue()
    {
        Assert.True(Builder().TryCreate(Max, out var result, out _));
        Assert.Equal(Max, result!.Value);
    }

    [Fact]
    public void TryCreate_ValidValue_HasEmptyError()
    {
        Builder().TryCreate(10, out _, out var error);
        Assert.Empty(error);
    }

    [Fact]
    public void Config_DifferentMax_EnforcesNewLimit()
    {
        var builder = StoryCount.Config(10);
        Assert.False(builder.TryCreate(11, out _, out _));
        Assert.True(builder.TryCreate(10, out _, out _));
    }
}
