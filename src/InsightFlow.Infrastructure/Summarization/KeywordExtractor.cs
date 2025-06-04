using System.Text.RegularExpressions;

namespace InsightFlow.Infrastructure.Summarization;

public sealed class KeywordExtractor
{
    private static readonly Regex TokenPattern = new(@"[a-zA-Z]{3,}", RegexOptions.Compiled);

    public IReadOnlyList<string> Extract(string text, int topN = 12)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();

        var tokens = TokenPattern.Matches(text.ToLowerInvariant())
            .Select(m => m.Value)
            .Where(t => !StopWords.Contains(t))
            .ToList();

        var total = tokens.Count;
        if (total == 0)
            return Array.Empty<string>();

        var termFrequency = tokens
            .GroupBy(t => t)
            .ToDictionary(g => g.Key, g => (double)g.Count() / total);

        var documentFrequency = termFrequency.Keys.ToDictionary(
            term => term,
            term => tokens.Count(t => t == term) > 0 ? 1.0 : 0.0);

        var scores = termFrequency.ToDictionary(
            pair => pair.Key,
            pair => pair.Value * Math.Log(1 + 1.0 / Math.Max(documentFrequency[pair.Key], 0.01)));

        return scores
            .OrderByDescending(s => s.Value)
            .Take(topN)
            .Select(s => s.Key)
            .ToList();
    }

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "that", "with", "this", "from", "are", "was", "were", "have", "has", "not", "but", "you", "your"
    };
}
