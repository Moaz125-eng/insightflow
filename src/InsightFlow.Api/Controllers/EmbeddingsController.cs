using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[ApiController]
[Route("api/documents/{documentId:guid}/embeddings")]
[Authorize(Policy = "AnalystOrAdmin")]
public sealed class EmbeddingsController : ControllerBase
{
    private readonly IEmbeddingService _embeddingService;

    public EmbeddingsController(IEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    [HttpPost]
    public async Task<ActionResult<EmbeddingBatchResponse>> Generate(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _embeddingService.GenerateForDocumentAsync(documentId, cancellationToken);
        return Ok(EmbeddingBatchResponse.From(result));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmbeddingSummary>>> List(Guid documentId, CancellationToken cancellationToken)
    {
        var vectors = await _embeddingService.GetEmbeddingsAsync(documentId, cancellationToken);
        return Ok(vectors.Select(EmbeddingSummary.From).ToList());
    }
}

public sealed record EmbeddingBatchResponse(
    Guid DocumentId,
    int TotalChunks,
    int EmbeddedChunks,
    int CacheHits,
    double DurationMs)
{
    public static EmbeddingBatchResponse From(EmbeddingBatchResult result) =>
        new(
            result.DocumentId,
            result.TotalChunks,
            result.EmbeddedChunks,
            result.CacheHits,
            result.Duration.TotalMilliseconds);
}

public sealed record EmbeddingSummary(int ChunkIndex, int Dimensions, string ModelVersion)
{
    public static EmbeddingSummary From(EmbeddingVector vector) =>
        new(vector.ChunkIndex, vector.Values.Length, vector.ModelVersion);
}
