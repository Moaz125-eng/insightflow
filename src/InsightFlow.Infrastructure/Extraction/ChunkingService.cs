using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Extraction;

public sealed class ChunkingService
{
    private const int DefaultChunkSize = 1200;
    private const int Overlap = 150;

    public IReadOnlyList<DocumentChunk> Chunk(string text, int chunkSize = DefaultChunkSize)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<DocumentChunk>();

        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal);
        var chunks = new List<DocumentChunk>();
        var index = 0;
        var position = 0;

        while (position < normalized.Length)
        {
            var length = Math.Min(chunkSize, normalized.Length - position);
            var slice = normalized.Substring(position, length);
            var end = position + slice.Length;

            chunks.Add(new DocumentChunk
            {
                Index = index,
                Text = slice.Trim(),
                StartOffset = position,
                EndOffset = end,
                TokenEstimate = EstimateTokens(slice)
            });

            if (end >= normalized.Length)
                break;

            position = Math.Max(0, end - Overlap);
            index++;
        }

        return chunks;
    }

    private static int EstimateTokens(string text) =>
        Math.Max(1, text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length);
}
