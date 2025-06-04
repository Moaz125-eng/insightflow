using FluentAssertions;
using InsightFlow.Infrastructure.Summarization;
using Xunit;

namespace InsightFlow.Tests;

public sealed class KeywordExtractorTests
{
    [Fact]
    public void Extract_returns_ranked_keywords()
    {
        var extractor = new KeywordExtractor();
        var text = "vector search enables semantic retrieval across document chunks and embeddings";
        var keywords = extractor.Extract(text, topN: 5);

        keywords.Should().NotBeEmpty();
        keywords.Should().OnlyHaveUniqueItems();
        keywords.Should().NotContain("the");
    }
}
