using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Qa;

public sealed class PromptAssembler
{
    public string Build(string question, IReadOnlyList<SearchResult> contexts)
    {
        var contextBlock = string.Join(
            "\n\n",
            contexts.Select((c, index) =>
                $"[source {index + 1} | doc {c.DocumentId:N} | chunk {c.ChunkIndex} | score {c.Score:F3}]\n{c.Snippet}"));

        return
$"""
You answer questions using only the supplied document context.

Question:
{question}

Context:
{contextBlock}

Instructions:
- Use only facts present in the context.
- If the context is insufficient, say you do not have enough information.
- Keep the answer concise and factual.
""";
    }
}
