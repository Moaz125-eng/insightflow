namespace InsightFlow.Core.Abstractions;

public interface IExportService
{
    Task<byte[]> ExportPdfReportAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<byte[]> ExportCsvAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<string> ExportMarkdownSummaryAsync(Guid documentId, CancellationToken cancellationToken = default);
}
