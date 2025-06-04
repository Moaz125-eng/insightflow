using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface ISummarizationService
{
    Task<DocumentSummary> SummarizeAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<DocumentSummary?> GetSummaryAsync(Guid documentId, CancellationToken cancellationToken = default);
}
