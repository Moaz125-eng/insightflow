namespace InsightFlow.Infrastructure.Extraction;

public sealed class TxtTextExtractor
{
    public async Task<(string Text, int PageCount)> ExtractAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var text = await reader.ReadToEndAsync(cancellationToken);
        var pages = Math.Max(1, text.Length / 3000);
        return (text, pages);
    }
}
