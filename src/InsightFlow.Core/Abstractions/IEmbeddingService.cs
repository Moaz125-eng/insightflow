using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface IEmbeddingService
{
    Task<EmbeddingBatchResult> GenerateForDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmbeddingVector>> GetEmbeddingsAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<EmbeddingVector?> GetChunkEmbeddingAsync(Guid documentId, int chunkIndex, CancellationToken cancellationToken = default);
}
