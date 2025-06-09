using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Background;

public sealed class BackgroundJobProcessor
{
    private readonly ITextExtractionService _extractionService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorSearchService _searchService;
    private readonly ISummarizationService _summarizationService;

    public BackgroundJobProcessor(
        ITextExtractionService extractionService,
        IEmbeddingService embeddingService,
        IVectorSearchService searchService,
        ISummarizationService summarizationService)
    {
        _extractionService = extractionService;
        _embeddingService = embeddingService;
        _searchService = searchService;
        _summarizationService = summarizationService;
    }

    public async Task ExecuteAsync(BackgroundJob job, CancellationToken cancellationToken)
    {
        switch (job.Type)
        {
            case BackgroundJobType.IndexDocument:
                await _extractionService.ExtractAsync(job.DocumentId, cancellationToken);
                await _searchService.IndexDocumentAsync(job.DocumentId, cancellationToken);
                break;
            case BackgroundJobType.GenerateEmbeddings:
                await _embeddingService.GenerateForDocumentAsync(job.DocumentId, cancellationToken);
                break;
            case BackgroundJobType.RunOcr:
                await _extractionService.ExtractAsync(job.DocumentId, cancellationToken);
                break;
            case BackgroundJobType.BuildSummary:
                await _summarizationService.SummarizeAsync(job.DocumentId, cancellationToken);
                break;
            default:
                throw new InvalidOperationException($"Unsupported job type: {job.Type}");
        }
    }
}
