using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[ApiController]
[Route("api/documents/{documentId:guid}/summary")]
public sealed class SummariesController : ControllerBase
{
    private readonly ISummarizationService _summarizationService;

    public SummariesController(ISummarizationService summarizationService)
    {
        _summarizationService = summarizationService;
    }

    [HttpPost]
    public async Task<ActionResult<SummaryResponse>> Generate(Guid documentId, CancellationToken cancellationToken)
    {
        var summary = await _summarizationService.SummarizeAsync(documentId, cancellationToken);
        return Ok(SummaryResponse.From(summary));
    }

    [HttpGet]
    public async Task<ActionResult<SummaryResponse>> Get(Guid documentId, CancellationToken cancellationToken)
    {
        var summary = await _summarizationService.GetSummaryAsync(documentId, cancellationToken);
        if (summary is null)
            return NotFound();

        return Ok(SummaryResponse.From(summary));
    }
}

public sealed record SummaryResponse(
    Guid DocumentId,
    string ShortSummary,
    string DetailedSummary,
    IReadOnlyList<string> Keywords,
    int ChunkCount,
    DateTimeOffset GeneratedAt)
{
    public static SummaryResponse From(DocumentSummary summary) =>
        new(
            summary.DocumentId,
            summary.ShortSummary,
            summary.DetailedSummary,
            summary.Keywords,
            summary.ChunkSummaries.Count,
            summary.GeneratedAt);
}
