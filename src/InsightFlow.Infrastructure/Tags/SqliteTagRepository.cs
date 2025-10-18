using InsightFlow.Core.Models;
using InsightFlow.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;

namespace InsightFlow.Infrastructure.Tags;

public sealed class SqliteTagRepository
{
    private readonly SqliteConnectionFactory _factory;

    public SqliteTagRepository(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<DocumentTag> CreateAsync(string name, string color, CancellationToken cancellationToken)
    {
        var tag = new DocumentTag
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Color = color.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO tags (id, name, color, created_at) VALUES ($id, $name, $color, $created_at)";
        command.Parameters.AddWithValue("$id", tag.Id.ToString());
        command.Parameters.AddWithValue("$name", tag.Name);
        command.Parameters.AddWithValue("$color", tag.Color);
        command.Parameters.AddWithValue("$created_at", tag.CreatedAt.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);

        return tag;
    }

    public async Task<IReadOnlyList<DocumentTag>> ListAsync(CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, color, created_at FROM tags ORDER BY name";

        var tags = new List<DocumentTag>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tags.Add(new DocumentTag
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.GetString(1),
                Color = reader.GetString(2),
                CreatedAt = DateTimeOffset.Parse(reader.GetString(3))
            });
        }

        return tags;
    }

    public async Task AssignAsync(Guid documentId, Guid tagId, CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO document_tags (document_id, tag_id) VALUES ($document_id, $tag_id)";
        command.Parameters.AddWithValue("$document_id", documentId.ToString());
        command.Parameters.AddWithValue("$tag_id", tagId.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid documentId, Guid tagId, CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM document_tags WHERE document_id = $document_id AND tag_id = $tag_id";
        command.Parameters.AddWithValue("$document_id", documentId.ToString());
        command.Parameters.AddWithValue("$tag_id", tagId.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentTag>> GetForDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
"""
SELECT t.id, t.name, t.color, t.created_at
FROM tags t
INNER JOIN document_tags dt ON dt.tag_id = t.id
WHERE dt.document_id = $document_id
ORDER BY t.name;
""";
        command.Parameters.AddWithValue("$document_id", documentId.ToString());

        var tags = new List<DocumentTag>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tags.Add(new DocumentTag
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.GetString(1),
                Color = reader.GetString(2),
                CreatedAt = DateTimeOffset.Parse(reader.GetString(3))
            });
        }

        return tags;
    }

    public async Task<IReadOnlyList<Guid>> GetDocumentIdsForTagAsync(Guid tagId, CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT document_id FROM document_tags WHERE tag_id = $tag_id";
        command.Parameters.AddWithValue("$tag_id", tagId.ToString());

        var ids = new List<Guid>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            ids.Add(Guid.Parse(reader.GetString(0)));

        return ids;
    }
}
