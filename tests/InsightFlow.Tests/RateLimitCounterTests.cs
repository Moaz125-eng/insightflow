using FluentAssertions;
using InsightFlow.Infrastructure.RateLimiting;
using Xunit;

namespace InsightFlow.Tests;

public sealed class RateLimitCounterTests
{
    [Fact]
    public void TryConsume_blocks_after_limit_within_window()
    {
        var counter = new RateLimitCounter();
        var window = TimeSpan.FromMinutes(1);

        counter.TryConsume("client-a", 2, window).Should().BeTrue();
        counter.TryConsume("client-a", 2, window).Should().BeTrue();
        counter.TryConsume("client-a", 2, window).Should().BeFalse();
        counter.TryConsume("client-b", 2, window).Should().BeTrue();
    }
}
