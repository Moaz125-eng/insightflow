namespace InsightFlow.Infrastructure.Summarization;

public sealed class HierarchicalSummarizer
{
    public (string ShortSummary, string DetailedSummary) RollUp(IReadOnlyList<string> chunkSummaries)
    {
        if (chunkSummaries.Count == 0)
            return ("No content available.", "No content available.");

        var detailed = string.Join(" ", chunkSummaries.Take(8));
        if (detailed.Length > 1200)
            detailed = detailed[..1200];

        var shortSentences = chunkSummaries
            .SelectMany(SplitSentences)
            .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        var shortSummary = shortSentences.Count == 0
            ? chunkSummaries[0][..Math.Min(220, chunkSummaries[0].Length)]
            : string.Join(" ", shortSentences);

        return (shortSummary.Trim(), detailed.Trim());
    }

    private static IEnumerable<string> SplitSentences(string text)
    {
        return text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 12);
    }
}
