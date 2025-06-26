using FluentAssertions;
using InsightFlow.Core.Models;
using InsightFlow.Infrastructure.Export;
using Xunit;

namespace InsightFlow.Tests;

public sealed class MarkdownSummaryExporterTests
{
    [Fact]
    public void Build_includes_summary_sections_and_keywords()
    {
        var exporter = new MarkdownSummaryExporter();
        var summary = new DocumentSummary
        {
            DocumentId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            ShortSummary = "Short overview",
            DetailedSummary = "Detailed overview with more context",
            Keywords = new[] { "vector", "search" },
            ChunkSummaries = new[]
            {
                new ChunkSummary { ChunkIndex = 0, Summary = "First chunk summary" }
            },
            GeneratedAt = DateTimeOffset.UtcNow
        };

        var markdown = exporter.Build(summary);
        markdown.Should().Contain("# InsightFlow Summary");
        markdown.Should().Contain("Short overview");
        markdown.Should().Contain("- vector");
        markdown.Should().Contain("### Chunk 0");
    }
}
