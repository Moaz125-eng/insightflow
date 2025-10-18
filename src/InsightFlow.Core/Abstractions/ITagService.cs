using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface ITagService
{
    Task<DocumentTag> CreateTagAsync(string name, string color, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentTag>> ListTagsAsync(CancellationToken cancellationToken = default);
    Task AssignTagAsync(Guid documentId, Guid tagId, CancellationToken cancellationToken = default);
    Task RemoveTagAsync(Guid documentId, Guid tagId, CancellationToken cancellationToken = default);
    Task<TaggedDocument?> GetDocumentTagsAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentRecord>> ListByTagAsync(Guid tagId, CancellationToken cancellationToken = default);
}
