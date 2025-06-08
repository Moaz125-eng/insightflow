using System.Text.RegularExpressions;

namespace InsightFlow.Infrastructure.Qa;

public sealed class LocalInferencePipeline
{
    private static readonly Regex QuestionTokenPattern = new(@"[a-zA-Z]{3,}", RegexOptions.Compiled);

    public (string Answer, float Confidence) Generate(string prompt)
    {
        var question = ExtractSection(prompt, "Question:");
        var context = ExtractSection(prompt, "Context:");

        var questionTokens = Tokenize(question);
        var contextLines = context.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !line.StartsWith("[source", StringComparison.Ordinal))
            .ToList();

        var scored = contextLines
            .Select(line => new
            {
                Line = line.Trim(),
                Score = OverlapScore(questionTokens, Tokenize(line))
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(3)
            .ToList();

        if (scored.Count == 0)
            return ("I do not have enough information in the indexed documents to answer that question.", 0.2f);

        var answer = string.Join(" ", scored.Select(s => s.Line));
        var confidence = Math.Min(0.95f, 0.45f + scored[0].Score / Math.Max(questionTokens.Count, 1));
        return (answer, confidence);
    }

    private static string ExtractSection(string prompt, string marker)
    {
        var start = prompt.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
            return string.Empty;

        start += marker.Length;
        var end = prompt.IndexOf("Instructions:", start, StringComparison.Ordinal);
        if (end < 0)
            end = prompt.Length;

        return prompt[start..end].Trim();
    }

    private static IReadOnlyList<string> Tokenize(string text) =>
        QuestionTokenPattern.Matches(text.ToLowerInvariant()).Select(m => m.Value).Distinct().ToList();

    private static int OverlapScore(IReadOnlyList<string> left, IReadOnlyList<string> right) =>
        left.Count(token => right.Contains(token));
}
