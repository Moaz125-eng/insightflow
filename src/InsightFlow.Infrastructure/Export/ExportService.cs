using InsightFlow.Core.Abstractions;

namespace InsightFlow.Infrastructure.Export;

public sealed class ExportService : IExportService
{
    private readonly ISummarizationService _summarizationService;
    private readonly IAnalyticsService _analyticsService;
    private readonly PdfReportExporter _pdfExporter = new();
    private readonly CsvAnalyticsExporter _csvExporter = new();
    private readonly MarkdownSummaryExporter _markdownExporter = new();

    public ExportService(ISummarizationService summarizationService, IAnalyticsService analyticsService)
    {
        _summarizationService = summarizationService;
        _analyticsService = analyticsService;
    }

    public async Task<byte[]> ExportPdfReportAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var summary = await _summarizationService.GetSummaryAsync(documentId, cancellationToken)
            ?? await _summarizationService.SummarizeAsync(documentId, cancellationToken);
        var analytics = await _analyticsService.GetSnapshotAsync(cancellationToken);
        return _pdfExporter.Build(summary, analytics);
    }

    public async Task<byte[]> ExportCsvAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var analytics = await _analyticsService.GetSnapshotAsync(cancellationToken);
        return _csvExporter.Build(analytics);
    }

    public async Task<string> ExportMarkdownSummaryAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var summary = await _summarizationService.GetSummaryAsync(documentId, cancellationToken)
            ?? await _summarizationService.SummarizeAsync(documentId, cancellationToken);
        return _markdownExporter.Build(summary);
    }
}
