using InsightFlow.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[ApiController]
[Route("api/export")]
[Authorize(Policy = "AnalystOrAdmin")]
public sealed class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpGet("documents/{documentId:guid}/pdf")]
    public async Task<IActionResult> ExportPdf(Guid documentId, CancellationToken cancellationToken)
    {
        var bytes = await _exportService.ExportPdfReportAsync(documentId, cancellationToken);
        return File(bytes, "application/pdf", $"insightflow-{documentId:N}.pdf");
    }

    [HttpGet("analytics/csv")]
    public async Task<IActionResult> ExportCsv(CancellationToken cancellationToken)
    {
        var bytes = await _exportService.ExportCsvAnalyticsAsync(cancellationToken);
        return File(bytes, "text/csv", $"insightflow-analytics-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("documents/{documentId:guid}/markdown")]
    public async Task<IActionResult> ExportMarkdown(Guid documentId, CancellationToken cancellationToken)
    {
        var markdown = await _exportService.ExportMarkdownSummaryAsync(documentId, cancellationToken);
        return Content(markdown, "text/markdown");
    }
}
