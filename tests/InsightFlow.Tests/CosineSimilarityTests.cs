using FluentAssertions;
using InsightFlow.Infrastructure.Search;
using Xunit;

namespace InsightFlow.Tests;

public sealed class CosineSimilarityTests
{
    [Fact]
    public void Identical_vectors_score_one()
    {
        var vector = new[] { 0.2f, 0.4f, 0.6f };
        CosineSimilarity.Compute(vector, vector).Should().BeApproximately(1f, 0.001f);
    }

    [Fact]
    public void Orthogonal_vectors_score_zero()
    {
        var left = new[] { 1f, 0f, 0f };
        var right = new[] { 0f, 1f, 0f };
        CosineSimilarity.Compute(left, right).Should().BeApproximately(0f, 0.001f);
    }
}
