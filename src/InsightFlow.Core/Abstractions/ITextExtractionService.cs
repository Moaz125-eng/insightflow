using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface ITextExtractionService
{
    Task<ExtractedDocument> ExtractAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<ExtractedDocument?> GetExtractedAsync(Guid documentId, CancellationToken cancellationToken = default);
}
