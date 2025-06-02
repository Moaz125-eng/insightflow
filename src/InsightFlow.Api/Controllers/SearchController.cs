using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[ApiController]
[Route("api/search")]
public sealed class SearchController : ControllerBase
{
    private readonly IVectorSearchService _searchService;

    public SearchController(IVectorSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpPost]
    public async Task<ActionResult<IReadOnlyList<SearchResult>>> Search(
        [FromBody] SearchRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest(new { error = "Query text is required." });

        var query = new SearchQuery
        {
            Text = request.Query,
            TopK = request.TopK ?? 10,
            MinimumScore = request.MinimumScore ?? 0.35f,
            DocumentId = request.DocumentId
        };

        var results = await _searchService.SearchAsync(query, cancellationToken);
        return Ok(results);
    }

    [HttpPost("index/{documentId:guid}")]
    public async Task<IActionResult> Index(Guid documentId, CancellationToken cancellationToken)
    {
        await _searchService.IndexDocumentAsync(documentId, cancellationToken);
        return Accepted();
    }
}

public sealed record SearchRequest(string Query, int? TopK, float? MinimumScore, Guid? DocumentId);
