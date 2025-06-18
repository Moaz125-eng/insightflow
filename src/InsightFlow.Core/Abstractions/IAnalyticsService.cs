using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface IAnalyticsService
{
    Task RecordSearchAsync(CancellationToken cancellationToken = default);
    Task<AnalyticsSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
