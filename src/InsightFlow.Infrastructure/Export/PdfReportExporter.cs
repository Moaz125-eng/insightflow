using InsightFlow.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InsightFlow.Infrastructure.Export;

public sealed class PdfReportExporter
{
    public PdfReportExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Build(DocumentSummary summary, AnalyticsSnapshot analytics)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header().Text("InsightFlow Report").FontSize(20).SemiBold();
                page.Content().Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Text($"Document: {summary.DocumentId}");
                    column.Item().Text($"Generated: {summary.GeneratedAt:u}");
                    column.Item().Text("Short Summary").FontSize(14).SemiBold();
                    column.Item().Text(summary.ShortSummary);
                    column.Item().Text("Detailed Summary").FontSize(14).SemiBold();
                    column.Item().Text(summary.DetailedSummary);
                    column.Item().Text("Keywords").FontSize(14).SemiBold();
                    column.Item().Text(string.Join(", ", summary.Keywords));
                    column.Item().Text("Analytics").FontSize(14).SemiBold();
                    column.Item().Text($"Indexed documents: {analytics.IndexedDocuments}");
                    column.Item().Text($"Total searches: {analytics.TotalSearches}");
                    column.Item().Text($"Embeddings: {analytics.TotalEmbeddings}");
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
}
