using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using InsightFlow.Infrastructure.Embeddings;

namespace InsightFlow.Infrastructure.Analytics;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly AnalyticsStore _store;
    private readonly IDocumentUploadService _uploadService;
    private readonly IEmbeddingService _embeddingService;
    private readonly EmbeddingCache _embeddingCache;

    public AnalyticsService(
        AnalyticsStore store,
        IDocumentUploadService uploadService,
        IEmbeddingService embeddingService,
        EmbeddingCache embeddingCache)
    {
        _store = store;
        _uploadService = uploadService;
        _embeddingService = embeddingService;
        _embeddingCache = embeddingCache;
    }

    public Task RecordSearchAsync(CancellationToken cancellationToken = default)
    {
        _store.RecordSearch();
        return Task.CompletedTask;
    }

    public async Task<AnalyticsSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _uploadService.ListAsync(cancellationToken);
        var indexed = documents.Count(d => d.Status == DocumentStatus.Indexed || d.Status == DocumentStatus.Processing);

        var embeddingTotals = 0;
        var dimensionTotal = 0.0;

        foreach (var document in documents)
        {
            var vectors = await _embeddingService.GetEmbeddingsAsync(document.Id, cancellationToken);
            embeddingTotals += vectors.Count;
            if (vectors.Count > 0)
                dimensionTotal += vectors.Average(v => v.Values.Length);
        }

        var averageDimensions = embeddingTotals == 0 ? 0 : dimensionTotal / embeddingTotals;

        return new AnalyticsSnapshot
        {
            IndexedDocuments = indexed,
            TotalSearches = _store.TotalSearches,
            TotalEmbeddings = embeddingTotals,
            AverageEmbeddingDimensions = averageDimensions,
            SearchFrequency = _store.GetDailySearchFrequency(),
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }
}
