using FluentAssertions;
using Xunit;

namespace InsightFlow.Tests;

public sealed class HealthEndpointTests
{
    [Fact]
    public void Placeholder_passes_until_integration_tests_added()
    {
        var expected = "healthy";
        expected.Should().Be("healthy");
    }
}
