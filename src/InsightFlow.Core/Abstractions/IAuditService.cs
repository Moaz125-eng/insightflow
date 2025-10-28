using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface IAuditService
{
    Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEntry>> ListRecentAsync(int take = 100, CancellationToken cancellationToken = default);
}
