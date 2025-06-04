using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Summarization;

public sealed class ChunkSummarizer
{
    public ChunkSummary Summarize(DocumentChunk chunk, int maxSentences = 2)
    {
        var sentences = SplitSentences(chunk.Text);
        var ranked = sentences
            .Select(s => new { Sentence = s, Score = ScoreSentence(s) })
            .OrderByDescending(x => x.Score)
            .Take(maxSentences)
            .Select(x => x.Sentence)
            .ToList();

        var summary = ranked.Count == 0 ? chunk.Text[..Math.Min(180, chunk.Text.Length)] : string.Join(" ", ranked);

        return new ChunkSummary
        {
            ChunkIndex = chunk.Index,
            Summary = summary.Trim()
        };
    }

    private static IReadOnlyList<string> SplitSentences(string text)
    {
        return text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 20)
            .ToList();
    }

    private static int ScoreSentence(string sentence)
    {
        var words = sentence.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var lengthScore = Math.Min(words.Length, 24);
        var numericBoost = sentence.Any(char.IsDigit) ? 2 : 0;
        return lengthScore + numericBoost;
    }
}
