using System.Collections.Concurrent;
using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Persistence;

public sealed class InMemoryDocumentRepository : IDocumentRepository
{
    private readonly ConcurrentDictionary<Guid, DocumentRecord> _store = new();

    public Task<DocumentRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var record);
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<DocumentRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DocumentRecord> items = _store.Values.OrderByDescending(d => d.UploadedAt).ToList();
        return Task.FromResult(items);
    }

    public Task SaveAsync(DocumentRecord document, CancellationToken cancellationToken = default)
    {
        _store[document.Id] = document;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
