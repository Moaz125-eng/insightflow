using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[ApiController]
[Route("api/tags")]
[Authorize(Policy = "AnalystOrAdmin")]
public sealed class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpPost]
    public async Task<ActionResult<TagResponse>> Create([FromBody] CreateTagRequest request, CancellationToken cancellationToken)
    {
        var tag = await _tagService.CreateTagAsync(request.Name, request.Color ?? "#4F46E5", cancellationToken);
        return CreatedAtAction(nameof(List), TagResponse.From(tag));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TagResponse>>> List(CancellationToken cancellationToken)
    {
        var tags = await _tagService.ListTagsAsync(cancellationToken);
        return Ok(tags.Select(TagResponse.From).ToList());
    }

    [HttpPost("documents/{documentId:guid}/{tagId:guid}")]
    public async Task<IActionResult> Assign(Guid documentId, Guid tagId, CancellationToken cancellationToken)
    {
        await _tagService.AssignTagAsync(documentId, tagId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("documents/{documentId:guid}/{tagId:guid}")]
    public async Task<IActionResult> Remove(Guid documentId, Guid tagId, CancellationToken cancellationToken)
    {
        await _tagService.RemoveTagAsync(documentId, tagId, cancellationToken);
        return NoContent();
    }

    [HttpGet("documents/{documentId:guid}")]
    public async Task<ActionResult<TaggedDocumentResponse>> GetForDocument(Guid documentId, CancellationToken cancellationToken)
    {
        var tagged = await _tagService.GetDocumentTagsAsync(documentId, cancellationToken);
        if (tagged is null)
            return NotFound();

        return Ok(TaggedDocumentResponse.From(tagged));
    }

    [HttpGet("{tagId:guid}/documents")]
    public async Task<ActionResult<IReadOnlyList<DocumentTagListItem>>> ListDocuments(Guid tagId, CancellationToken cancellationToken)
    {
        var documents = await _tagService.ListByTagAsync(tagId, cancellationToken);
        return Ok(documents.Select(DocumentTagListItem.From).ToList());
    }
}

public sealed record CreateTagRequest(string Name, string? Color);

public sealed record TagResponse(Guid Id, string Name, string Color)
{
    public static TagResponse From(DocumentTag tag) => new(tag.Id, tag.Name, tag.Color);
}

public sealed record TaggedDocumentResponse(Guid DocumentId, IReadOnlyList<TagResponse> Tags)
{
    public static TaggedDocumentResponse From(TaggedDocument tagged) =>
        new(tagged.DocumentId, tagged.Tags.Select(TagResponse.From).ToList());
}

public sealed record DocumentTagListItem(Guid Id, string FileName, string Status)
{
    public static DocumentTagListItem From(DocumentRecord record) =>
        new(record.Id, record.FileName, record.Status.ToString());
}
