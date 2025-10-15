using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.Data.Sqlite;

namespace InsightFlow.Infrastructure.Persistence;

public sealed class SqliteDocumentRepository : IDocumentRepository
{
    private readonly SqliteConnectionFactory _factory;

    public SqliteDocumentRepository(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<DocumentRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, file_name, content_type, size_bytes, storage_path, status, uploaded_at FROM documents WHERE id = $id";
        command.Parameters.AddWithValue("$id", id.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return Map(reader);
    }

    public async Task<IReadOnlyList<DocumentRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, file_name, content_type, size_bytes, storage_path, status, uploaded_at FROM documents ORDER BY uploaded_at DESC";

        var results = new List<DocumentRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            results.Add(Map(reader));

        return results;
    }

    public async Task SaveAsync(DocumentRecord document, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
"""
INSERT INTO documents (id, file_name, content_type, size_bytes, storage_path, status, uploaded_at)
VALUES ($id, $file_name, $content_type, $size_bytes, $storage_path, $status, $uploaded_at)
ON CONFLICT(id) DO UPDATE SET
    file_name = excluded.file_name,
    content_type = excluded.content_type,
    size_bytes = excluded.size_bytes,
    storage_path = excluded.storage_path,
    status = excluded.status,
    uploaded_at = excluded.uploaded_at;
""";
        command.Parameters.AddWithValue("$id", document.Id.ToString());
        command.Parameters.AddWithValue("$file_name", document.FileName);
        command.Parameters.AddWithValue("$content_type", document.ContentType);
        command.Parameters.AddWithValue("$size_bytes", document.SizeBytes);
        command.Parameters.AddWithValue("$storage_path", document.StoragePath);
        command.Parameters.AddWithValue("$status", document.Status.ToString());
        command.Parameters.AddWithValue("$uploaded_at", document.UploadedAt.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM documents WHERE id = $id";
        command.Parameters.AddWithValue("$id", id.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static DocumentRecord Map(SqliteDataReader reader) =>
        new()
        {
            Id = Guid.Parse(reader.GetString(0)),
            FileName = reader.GetString(1),
            ContentType = reader.GetString(2),
            SizeBytes = reader.GetInt64(3),
            StoragePath = reader.GetString(4),
            Status = Enum.Parse<DocumentStatus>(reader.GetString(5)),
            UploadedAt = DateTimeOffset.Parse(reader.GetString(6))
        };
}
