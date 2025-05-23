using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface IDocumentRepository
{
    Task<DocumentRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(DocumentRecord document, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
