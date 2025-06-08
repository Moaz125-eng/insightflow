using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Qa;

public sealed class RagRetriever
{
    private readonly IVectorSearchService _searchService;

    public RagRetriever(IVectorSearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<IReadOnlyList<SearchResult>> RetrieveAsync(
        QuestionRequest request,
        CancellationToken cancellationToken)
    {
        var query = new SearchQuery
        {
            Text = request.Question,
            TopK = request.ContextChunks,
            MinimumScore = 0.25f,
            DocumentId = request.DocumentId
        };

        return await _searchService.SearchAsync(query, cancellationToken);
    }
}
