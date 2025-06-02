using System.Collections.Concurrent;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Search;

public sealed class VectorIndex
{
    private readonly ConcurrentDictionary<Guid, List<IndexedChunk>> _documents = new();

    public void Upsert(Guid documentId, IReadOnlyList<(int ChunkIndex, float[] Vector, string Snippet)> chunks)
    {
        var indexed = chunks.Select(c => new IndexedChunk
        {
            ChunkIndex = c.ChunkIndex,
            Vector = c.Vector,
            Snippet = c.Snippet
        }).ToList();

        _documents[documentId] = indexed;
    }

    public IEnumerable<(Guid DocumentId, IndexedChunk Chunk)> Enumerate(Guid? filterDocumentId = null)
    {
        foreach (var pair in _documents)
        {
            if (filterDocumentId.HasValue && pair.Key != filterDocumentId.Value)
                continue;

            foreach (var chunk in pair.Value)
                yield return (pair.Key, chunk);
        }
    }

    public bool Contains(Guid documentId) => _documents.ContainsKey(documentId);
}

public sealed class IndexedChunk
{
    public required int ChunkIndex { get; init; }
    public required float[] Vector { get; init; }
    public required string Snippet { get; init; }
}
