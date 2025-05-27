using FluentAssertions;
using InsightFlow.Infrastructure.Extraction;
using Xunit;

namespace InsightFlow.Tests;

public sealed class ChunkingServiceTests
{
    [Fact]
    public void Chunk_splits_long_text_with_overlap()
    {
        var service = new ChunkingService();
        var text = new string('a', 2500);
        var chunks = service.Chunk(text, chunkSize: 1000);

        chunks.Should().HaveCountGreaterThan(1);
        chunks[0].StartOffset.Should().Be(0);
        chunks[^1].EndOffset.Should().BeLessOrEqualTo(text.Length);
        chunks.Select(c => c.Index).Should().BeInAscendingOrder();
    }

    [Fact]
    public void Chunk_returns_empty_for_blank_input()
    {
        var service = new ChunkingService();
        service.Chunk("   ").Should().BeEmpty();
    }
}
