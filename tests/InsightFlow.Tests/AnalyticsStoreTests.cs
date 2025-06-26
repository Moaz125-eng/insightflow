using FluentAssertions;
using InsightFlow.Infrastructure.Analytics;
using Xunit;

namespace InsightFlow.Tests;

public sealed class AnalyticsStoreTests
{
    [Fact]
    public void RecordSearch_increments_totals_and_daily_metrics()
    {
        var store = new AnalyticsStore();
        store.RecordSearch();
        store.RecordSearch();

        store.TotalSearches.Should().Be(2);
        store.GetDailySearchFrequency().Should().Contain(m => m.Count >= 2);
    }
}
