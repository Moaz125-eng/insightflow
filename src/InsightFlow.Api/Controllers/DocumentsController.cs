using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize(Policy = "AnalystOrAdmin")]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentUploadService _uploadService;

    public DocumentsController(IDocumentUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    [HttpPost]
    [RequestSizeLimit(52_428_800)]
    public async Task<ActionResult<DocumentResponse>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        await using var stream = file.OpenReadStream();
        var record = await _uploadService.UploadAsync(
            stream,
            file.FileName,
            file.ContentType ?? "application/octet-stream",
            file.Length,
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = record.Id }, DocumentResponse.From(record));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentResponse>>> List(CancellationToken cancellationToken)
    {
        var items = await _uploadService.ListAsync(cancellationToken);
        return Ok(items.Select(DocumentResponse.From).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var record = await _uploadService.GetAsync(id, cancellationToken);
        if (record is null)
            return NotFound();

        return Ok(DocumentResponse.From(record));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var record = await _uploadService.GetAsync(id, cancellationToken);
        if (record is null)
            return NotFound();

        await _uploadService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

public sealed record DocumentResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Status,
    DateTimeOffset UploadedAt)
{
    public static DocumentResponse From(DocumentRecord record) =>
        new(
            record.Id,
            record.FileName,
            record.ContentType,
            record.SizeBytes,
            record.Status.ToString(),
            record.UploadedAt);
}
