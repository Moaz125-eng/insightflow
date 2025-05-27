using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace InsightFlow.Infrastructure.Extraction;

public sealed class DocxTextExtractor
{
    public (string Text, int PageCount) Extract(Stream stream)
    {
        using var package = WordprocessingDocument.Open(stream, false);
        var body = package.MainDocumentPart?.Document?.Body;
        if (body is null)
            return (string.Empty, 0);

        var paragraphs = body.Descendants<Paragraph>()
            .Select(p => p.InnerText?.Trim() ?? string.Empty)
            .Where(t => !string.IsNullOrWhiteSpace(t));

        var text = string.Join("\n", paragraphs);
        var pageEstimate = Math.Max(1, text.Length / 3000);
        return (text, pageEstimate);
    }
}
