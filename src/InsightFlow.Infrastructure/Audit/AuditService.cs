using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Audit;

public sealed class AuditService : IAuditService
{
    private readonly SqliteAuditStore _store;

    public AuditService(SqliteAuditStore store)
    {
        _store = store;
    }

    public Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken = default) =>
        _store.SaveAsync(entry, cancellationToken);

    public Task<IReadOnlyList<AuditEntry>> ListRecentAsync(int take = 100, CancellationToken cancellationToken = default) =>
        _store.ListRecentAsync(take, cancellationToken);
}
