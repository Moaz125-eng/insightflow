using UglyToad.PdfPig;

namespace InsightFlow.Infrastructure.Extraction;

public sealed class PdfTextExtractor
{
    public (string Text, int PageCount) Extract(Stream stream)
    {
        using var document = PdfDocument.Open(stream);
        var pages = new List<string>();

        foreach (var page in document.GetPages())
        {
            var pageText = page.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(pageText))
                pages.Add(pageText);
        }

        var combined = string.Join("\n\n", pages);
        return (combined, document.NumberOfPages);
    }
}
