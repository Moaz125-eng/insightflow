using System.Collections.Concurrent;

namespace InsightFlow.Infrastructure.RateLimiting;

public sealed class RateLimitCounter
{
    private readonly ConcurrentDictionary<string, WindowState> _windows = new();

    public bool TryConsume(string key, int limit, TimeSpan window)
    {
        var now = DateTimeOffset.UtcNow;
        var state = _windows.AddOrUpdate(
            key,
            _ => new WindowState(now, 1),
            (_, existing) =>
            {
                if (now - existing.WindowStart >= window)
                    return new WindowState(now, 1);

                return existing with { Count = existing.Count + 1 };
            });

        if (now - state.WindowStart >= window)
            return true;

        return state.Count <= limit;
    }

    private sealed record WindowState(DateTimeOffset WindowStart, int Count);
}
