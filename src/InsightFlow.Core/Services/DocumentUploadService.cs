using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using InsightFlow.Core.Validation;

namespace InsightFlow.Core.Services;

public sealed class DocumentUploadService : IDocumentUploadService
{
    private readonly IDocumentRepository _repository;
    private readonly IDocumentStorage _storage;

    public DocumentUploadService(IDocumentRepository repository, IDocumentStorage storage)
    {
        _repository = repository;
        _storage = storage;
    }

    public async Task<DocumentRecord> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken = default)
    {
        var validation = UploadValidation.Validate(fileName, contentType, sizeBytes);
        if (!validation.IsValid)
            throw new InvalidOperationException(validation.ErrorMessage);

        var storagePath = await _storage.SaveAsync(fileStream, fileName, cancellationToken);

        var record = new DocumentRecord
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            StoragePath = storagePath,
            Status = DocumentStatus.Pending,
            UploadedAt = DateTimeOffset.UtcNow
        };

        await _repository.SaveAsync(record, cancellationToken);
        return record;
    }

    public Task<DocumentRecord?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<DocumentRecord>> ListAsync(CancellationToken cancellationToken = default) =>
        _repository.ListAsync(cancellationToken);

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return;

        await _storage.DeleteFileAsync(existing.StoragePath, cancellationToken);
        await _repository.DeleteAsync(id, cancellationToken);
    }
}
