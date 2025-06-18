using System.Collections.Concurrent;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Analytics;

public sealed class AnalyticsStore
{
    private int _searchCount;
    private readonly ConcurrentDictionary<DateOnly, int> _dailySearches = new();

    public void RecordSearch()
    {
        Interlocked.Increment(ref _searchCount);
        var day = DateOnly.FromDateTime(DateTime.UtcNow);
        _dailySearches.AddOrUpdate(day, 1, (_, current) => current + 1);
    }

    public int TotalSearches => _searchCount;

    public IReadOnlyList<DailyMetric> GetDailySearchFrequency(int days = 14)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var metrics = new List<DailyMetric>();

        for (var offset = days - 1; offset >= 0; offset--)
        {
            var date = today.AddDays(-offset);
            _dailySearches.TryGetValue(date, out var count);
            metrics.Add(new DailyMetric { Date = date, Count = count });
        }

        return metrics;
    }
}
