using InsightFlow.Core.Models;
using System.Text;

namespace InsightFlow.Infrastructure.Export;

public sealed class MarkdownSummaryExporter
{
    public string Build(DocumentSummary summary)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# InsightFlow Summary");
        builder.AppendLine();
        builder.AppendLine($"**Document:** `{summary.DocumentId}`  ");
        builder.AppendLine($"**Generated:** {summary.GeneratedAt:u}  ");
        builder.AppendLine();
        builder.AppendLine("## Short Summary");
        builder.AppendLine(summary.ShortSummary);
        builder.AppendLine();
        builder.AppendLine("## Detailed Summary");
        builder.AppendLine(summary.DetailedSummary);
        builder.AppendLine();
        builder.AppendLine("## Keywords");
        foreach (var keyword in summary.Keywords)
            builder.AppendLine($"- {keyword}");

        builder.AppendLine();
        builder.AppendLine("## Chunk Summaries");
        foreach (var chunk in summary.ChunkSummaries.OrderBy(c => c.ChunkIndex))
        {
            builder.AppendLine($"### Chunk {chunk.ChunkIndex}");
            builder.AppendLine(chunk.Summary);
            builder.AppendLine();
        }

        return builder.ToString();
    }
}
