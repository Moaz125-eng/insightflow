using FluentAssertions;
using InsightFlow.Core.Models;
using InsightFlow.Infrastructure.Embeddings;
using Xunit;

namespace InsightFlow.Tests;

public sealed class EmbeddingCacheTests
{
    [Fact]
    public void Cache_persists_and_reloads_vectors()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var cache = new EmbeddingCache(root);
        var documentId = Guid.NewGuid();

        var vector = new EmbeddingVector
        {
            DocumentId = documentId,
            ChunkIndex = 0,
            Values = new[] { 0.1f, 0.2f, 0.3f },
            ModelVersion = "test",
            CreatedAt = DateTimeOffset.UtcNow
        };

        cache.Set(vector);
        cache.TryGet(documentId, 0, out var loaded).Should().BeTrue();
        loaded!.Values.Should().BeEquivalentTo(vector.Values);

        var secondCache = new EmbeddingCache(root);
        secondCache.TryGet(documentId, 0, out var reloaded).Should().BeTrue();
        reloaded!.ChunkIndex.Should().Be(0);
    }
}
