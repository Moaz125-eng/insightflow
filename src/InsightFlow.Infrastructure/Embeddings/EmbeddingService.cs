using System.Collections.Concurrent;
using System.Diagnostics;
using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Embeddings;

public sealed class EmbeddingService : IEmbeddingService, IDisposable
{
    private readonly ITextExtractionService _extractionService;
    private readonly BatchEmbeddingProcessor _processor;
    private readonly OnnxEmbeddingRuntime _runtime;
    private readonly ConcurrentDictionary<Guid, IReadOnlyList<EmbeddingVector>> _store = new();

    public EmbeddingService(
        ITextExtractionService extractionService,
        OnnxEmbeddingRuntime runtime,
        EmbeddingCache cache)
    {
        _extractionService = extractionService;
        _runtime = runtime;
        _processor = new BatchEmbeddingProcessor(runtime, cache);
    }

    public async Task<EmbeddingBatchResult> GenerateForDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var extracted = await _extractionService.GetExtractedAsync(documentId, cancellationToken)
            ?? await _extractionService.ExtractAsync(documentId, cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var (vectors, cacheHits) = await _processor.ProcessAsync(
            documentId,
            extracted.Chunks,
            _runtime.ModelVersion,
            cancellationToken);

        stopwatch.Stop();
        _store[documentId] = vectors;

        return new EmbeddingBatchResult
        {
            DocumentId = documentId,
            TotalChunks = extracted.Chunks.Count,
            EmbeddedChunks = vectors.Count,
            CacheHits = cacheHits,
            Duration = stopwatch.Elapsed
        };
    }

    public Task<IReadOnlyList<EmbeddingVector>> GetEmbeddingsAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(documentId, out var vectors);
        return Task.FromResult<IReadOnlyList<EmbeddingVector>>(vectors ?? Array.Empty<EmbeddingVector>());
    }

    public async Task<EmbeddingVector?> GetChunkEmbeddingAsync(
        Guid documentId,
        int chunkIndex,
        CancellationToken cancellationToken = default)
    {
        var vectors = await GetEmbeddingsAsync(documentId, cancellationToken);
        return vectors.FirstOrDefault(v => v.ChunkIndex == chunkIndex);
    }

    public void Dispose() => _runtime.Dispose();
}
