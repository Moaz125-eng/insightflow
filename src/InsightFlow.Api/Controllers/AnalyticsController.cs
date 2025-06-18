using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize(Policy = "AnalystOrAdmin")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet]
    public async Task<ActionResult<AnalyticsSnapshot>> GetSnapshot(CancellationToken cancellationToken)
    {
        var snapshot = await _analyticsService.GetSnapshotAsync(cancellationToken);
        return Ok(snapshot);
    }
}
