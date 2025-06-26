using FluentAssertions;
using InsightFlow.Infrastructure.Qa;
using Xunit;

namespace InsightFlow.Tests;

public sealed class LocalInferencePipelineTests
{
    [Fact]
    public void Generate_returns_answer_from_matching_context()
    {
        var pipeline = new LocalInferencePipeline();
        var prompt =
"""
Question:
What is vector search?

Context:
[source 1 | doc 00000000-0000-0000-0000-000000000001 | chunk 0 | score 0.812]
vector search enables semantic retrieval across document chunks

Instructions:
- Use only facts present in the context.
""";

        var (answer, confidence) = pipeline.Generate(prompt);
        answer.Should().Contain("vector search");
        confidence.Should().BeGreaterThan(0.4f);
    }

    [Fact]
    public void Generate_returns_insufficient_information_without_context()
    {
        var pipeline = new LocalInferencePipeline();
        var prompt =
"""
Question:
What is the revenue forecast?

Context:


Instructions:
- Use only facts present in the context.
""";

        var (answer, confidence) = pipeline.Generate(prompt);
        answer.Should().Contain("enough information");
        confidence.Should().BeLessThan(0.3f);
    }
}
