using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[ApiController]
[Route("api/documents/{documentId:guid}/extraction")]
[Authorize(Policy = "AnalystOrAdmin")]
public sealed class ExtractionController : ControllerBase
{
    private readonly ITextExtractionService _extractionService;

    public ExtractionController(ITextExtractionService extractionService)
    {
        _extractionService = extractionService;
    }

    [HttpPost]
    public async Task<ActionResult<ExtractionResponse>> Extract(Guid documentId, CancellationToken cancellationToken)
    {
        var extracted = await _extractionService.ExtractAsync(documentId, cancellationToken);
        return Ok(ExtractionResponse.From(extracted));
    }

    [HttpGet]
    public async Task<ActionResult<ExtractionResponse>> Get(Guid documentId, CancellationToken cancellationToken)
    {
        var extracted = await _extractionService.GetExtractedAsync(documentId, cancellationToken);
        if (extracted is null)
            return NotFound();

        return Ok(ExtractionResponse.From(extracted));
    }
}

public sealed record ExtractionResponse(
    Guid DocumentId,
    int ChunkCount,
    int WordCount,
    int PageCount,
    string Title,
    string SourceFormat,
    DateTimeOffset ExtractedAt)
{
    public static ExtractionResponse From(ExtractedDocument doc) =>
        new(
            doc.DocumentId,
            doc.Chunks.Count,
            doc.Metadata.WordCount,
            doc.Metadata.PageCount,
            doc.Metadata.Title,
            doc.Metadata.SourceFormat,
            doc.ExtractedAt);
}
