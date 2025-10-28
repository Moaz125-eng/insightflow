using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Policy = "AdminOnly")]
public sealed class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditEntryResponse>>> List(
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var boundedTake = Math.Clamp(take, 1, 500);
        var entries = await _auditService.ListRecentAsync(boundedTake, cancellationToken);
        return Ok(entries.Select(AuditEntryResponse.From).ToList());
    }
}

public sealed record AuditEntryResponse(
    Guid Id,
    string Actor,
    string Action,
    string Resource,
    string Method,
    int StatusCode,
    DateTimeOffset OccurredAt)
{
    public static AuditEntryResponse From(AuditEntry entry) =>
        new(entry.Id, entry.Actor, entry.Action, entry.Resource, entry.Method, entry.StatusCode, entry.OccurredAt);
}
