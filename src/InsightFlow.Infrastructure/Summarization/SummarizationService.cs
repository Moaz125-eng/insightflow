using System.Collections.Concurrent;
using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Summarization;

public sealed class SummarizationService : ISummarizationService
{
    private readonly ITextExtractionService _extractionService;
    private readonly ChunkSummarizer _chunkSummarizer = new();
    private readonly HierarchicalSummarizer _hierarchical = new();
    private readonly KeywordExtractor _keywords = new();
    private readonly ConcurrentDictionary<Guid, DocumentSummary> _cache = new();

    public SummarizationService(ITextExtractionService extractionService)
    {
        _extractionService = extractionService;
    }

    public async Task<DocumentSummary> SummarizeAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var extracted = await _extractionService.GetExtractedAsync(documentId, cancellationToken)
            ?? await _extractionService.ExtractAsync(documentId, cancellationToken);

        var chunkSummaries = extracted.Chunks
            .Select(chunk => _chunkSummarizer.Summarize(chunk))
            .ToList();

        var rollupTexts = chunkSummaries.Select(c => c.Summary).ToList();
        var (shortSummary, detailedSummary) = _hierarchical.RollUp(rollupTexts);
        var keywordList = _keywords.Extract(extracted.FullText);

        var summary = new DocumentSummary
        {
            DocumentId = documentId,
            ShortSummary = shortSummary,
            DetailedSummary = detailedSummary,
            Keywords = keywordList,
            ChunkSummaries = chunkSummaries,
            GeneratedAt = DateTimeOffset.UtcNow
        };

        _cache[documentId] = summary;
        return summary;
    }

    public Task<DocumentSummary?> GetSummaryAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(documentId, out var summary);
        return Task.FromResult(summary);
    }
}
