using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface IVectorSearchService
{
    Task<IReadOnlyList<SearchResult>> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);
    Task IndexDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
}
