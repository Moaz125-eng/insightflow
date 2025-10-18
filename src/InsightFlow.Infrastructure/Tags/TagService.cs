using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Tags;

public sealed class TagService : ITagService
{
    private readonly SqliteTagRepository _repository;
    private readonly IDocumentRepository _documents;

    public TagService(SqliteTagRepository repository, IDocumentRepository documents)
    {
        _repository = repository;
        _documents = documents;
    }

    public Task<DocumentTag> CreateTagAsync(string name, string color, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Tag name is required.");

        var normalizedColor = string.IsNullOrWhiteSpace(color) ? "#4F46E5" : color;
        return _repository.CreateAsync(name, normalizedColor, cancellationToken);
    }

    public Task<IReadOnlyList<DocumentTag>> ListTagsAsync(CancellationToken cancellationToken = default) =>
        _repository.ListAsync(cancellationToken);

    public async Task AssignTagAsync(Guid documentId, Guid tagId, CancellationToken cancellationToken = default)
    {
        if (await _documents.GetByIdAsync(documentId, cancellationToken) is null)
            throw new InvalidOperationException("Document not found.");

        await _repository.AssignAsync(documentId, tagId, cancellationToken);
    }

    public Task RemoveTagAsync(Guid documentId, Guid tagId, CancellationToken cancellationToken = default) =>
        _repository.RemoveAsync(documentId, tagId, cancellationToken);

    public async Task<TaggedDocument?> GetDocumentTagsAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        if (await _documents.GetByIdAsync(documentId, cancellationToken) is null)
            return null;

        var tags = await _repository.GetForDocumentAsync(documentId, cancellationToken);
        return new TaggedDocument { DocumentId = documentId, Tags = tags };
    }

    public async Task<IReadOnlyList<DocumentRecord>> ListByTagAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        var documentIds = await _repository.GetDocumentIdsForTagAsync(tagId, cancellationToken);
        var records = new List<DocumentRecord>();

        foreach (var id in documentIds)
        {
            var record = await _documents.GetByIdAsync(id, cancellationToken);
            if (record is not null)
                records.Add(record);
        }

        return records;
    }
}
