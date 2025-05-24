using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface IDocumentUploadService
{
    Task<DocumentRecord> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken = default);

    Task<DocumentRecord?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
