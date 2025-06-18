using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using InsightFlow.Infrastructure.Embeddings;

namespace InsightFlow.Infrastructure.Search;

public sealed class VectorSearchService : IVectorSearchService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly ITextExtractionService _extractionService;
    private readonly OnnxEmbeddingRuntime _runtime;
    private readonly Analytics.AnalyticsStore? _analyticsStore;
    private readonly VectorIndex _index = new();

    public VectorSearchService(
        IEmbeddingService embeddingService,
        ITextExtractionService extractionService,
        OnnxEmbeddingRuntime runtime,
        Analytics.AnalyticsStore analyticsStore)
    {
        _embeddingService = embeddingService;
        _extractionService = extractionService;
        _runtime = runtime;
        _analyticsStore = analyticsStore;
    }

    public async Task IndexDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        await _embeddingService.GenerateForDocumentAsync(documentId, cancellationToken);
        var extracted = await _extractionService.GetExtractedAsync(documentId, cancellationToken);
        var embeddings = await _embeddingService.GetEmbeddingsAsync(documentId, cancellationToken);

        if (extracted is null || embeddings.Count == 0)
            return;

        var chunks = embeddings.Select(e =>
        {
            var snippet = extracted.Chunks.FirstOrDefault(c => c.Index == e.ChunkIndex)?.Text ?? string.Empty;
            var trimmed = snippet.Length > 240 ? snippet[..240] + "..." : snippet;
            return (e.ChunkIndex, e.Values, trimmed);
        }).ToList();

        _index.Upsert(documentId, chunks);
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        _analyticsStore?.RecordSearch();
        var queryVector = _runtime.Encode(query.Text);
        var candidates = new List<SearchResult>();

        foreach (var (documentId, chunk) in _index.Enumerate(query.DocumentId))
        {
            var score = CosineSimilarity.Compute(queryVector, chunk.Vector);
            if (score < query.MinimumScore)
                continue;

            candidates.Add(new SearchResult
            {
                DocumentId = documentId,
                ChunkIndex = chunk.ChunkIndex,
                Snippet = chunk.Snippet,
                Score = score,
                Rank = 0
            });
        }

        return candidates
            .OrderByDescending(c => c.Score)
            .Take(query.TopK)
            .Select((item, index) => new SearchResult
            {
                DocumentId = item.DocumentId,
                ChunkIndex = item.ChunkIndex,
                Snippet = item.Snippet,
                Score = item.Score,
                Rank = index + 1
            })
            .ToList();
    }
}
