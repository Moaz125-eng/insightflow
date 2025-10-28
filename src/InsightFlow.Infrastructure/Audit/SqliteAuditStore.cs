using InsightFlow.Core.Models;
using InsightFlow.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;

namespace InsightFlow.Infrastructure.Audit;

public sealed class SqliteAuditStore
{
    private readonly SqliteConnectionFactory _factory;

    public SqliteAuditStore(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task SaveAsync(AuditEntry entry, CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
"""
INSERT INTO audit_log (id, actor, action, resource, method, status_code, occurred_at)
VALUES ($id, $actor, $action, $resource, $method, $status_code, $occurred_at);
""";
        command.Parameters.AddWithValue("$id", entry.Id.ToString());
        command.Parameters.AddWithValue("$actor", entry.Actor);
        command.Parameters.AddWithValue("$action", entry.Action);
        command.Parameters.AddWithValue("$resource", entry.Resource);
        command.Parameters.AddWithValue("$method", entry.Method);
        command.Parameters.AddWithValue("$status_code", entry.StatusCode);
        command.Parameters.AddWithValue("$occurred_at", entry.OccurredAt.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditEntry>> ListRecentAsync(int take, CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
"""
SELECT id, actor, action, resource, method, status_code, occurred_at
FROM audit_log
ORDER BY occurred_at DESC
LIMIT $take;
""";
        command.Parameters.AddWithValue("$take", take);

        var entries = new List<AuditEntry>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(new AuditEntry
            {
                Id = Guid.Parse(reader.GetString(0)),
                Actor = reader.GetString(1),
                Action = reader.GetString(2),
                Resource = reader.GetString(3),
                Method = reader.GetString(4),
                StatusCode = reader.GetInt32(5),
                OccurredAt = DateTimeOffset.Parse(reader.GetString(6))
            });
        }

        return entries;
    }
}
