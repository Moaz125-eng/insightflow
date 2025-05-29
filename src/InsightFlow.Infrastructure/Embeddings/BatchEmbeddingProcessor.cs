using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Embeddings;

public sealed class BatchEmbeddingProcessor
{
    private readonly OnnxEmbeddingRuntime _runtime;
    private readonly EmbeddingCache _cache;
    private const int BatchSize = 16;

    public BatchEmbeddingProcessor(OnnxEmbeddingRuntime runtime, EmbeddingCache cache)
    {
        _runtime = runtime;
        _cache = cache;
    }

    public async Task<(IReadOnlyList<EmbeddingVector> Vectors, int CacheHits)> ProcessAsync(
        Guid documentId,
        IReadOnlyList<DocumentChunk> chunks,
        string modelVersion,
        CancellationToken cancellationToken)
    {
        var results = new List<EmbeddingVector>();
        var cacheHits = 0;
        var pending = new List<(int Index, string Text)>();

        foreach (var chunk in chunks)
        {
            if (_cache.TryGet(documentId, chunk.Index, out var cached) && cached is not null)
            {
                results.Add(cached);
                cacheHits++;
                continue;
            }

            pending.Add((chunk.Index, chunk.Text));
        }

        for (var offset = 0; offset < pending.Count; offset += BatchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var slice = pending.Skip(offset).Take(BatchSize).ToList();
            var texts = slice.Select(s => s.Text).ToList();
            var vectors = _runtime.EncodeBatch(texts);

            for (var i = 0; i < slice.Count; i++)
            {
                var vector = new EmbeddingVector
                {
                    DocumentId = documentId,
                    ChunkIndex = slice[i].Index,
                    Values = vectors[i],
                    ModelVersion = modelVersion,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _cache.Set(vector);
                results.Add(vector);
            }

            await Task.Yield();
        }

        return (results.OrderBy(v => v.ChunkIndex).ToList(), cacheHits);
    }
}
