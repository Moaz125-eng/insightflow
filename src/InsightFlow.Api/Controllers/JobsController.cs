using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightFlow.Api.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize(Policy = "AdminOnly")]
public sealed class JobsController : ControllerBase
{
    private readonly IBackgroundJobQueue _queue;

    public JobsController(IBackgroundJobQueue queue)
    {
        _queue = queue;
    }

    [HttpPost]
    public async Task<ActionResult<JobResponse>> Enqueue([FromBody] EnqueueJobRequest request, CancellationToken cancellationToken)
    {
        var job = new BackgroundJob
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            DocumentId = request.DocumentId,
            Status = BackgroundJobStatus.Queued,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _queue.EnqueueAsync(job, cancellationToken);
        return AcceptedAtAction(nameof(GetById), new { id = job.Id }, JobResponse.From(job));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var job = await _queue.GetAsync(id, cancellationToken);
        if (job is null)
            return NotFound();

        return Ok(JobResponse.From(job));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<JobResponse>>> List(CancellationToken cancellationToken)
    {
        var jobs = await _queue.ListAsync(cancellationToken);
        return Ok(jobs.Select(JobResponse.From).ToList());
    }
}

public sealed record EnqueueJobRequest(BackgroundJobType Type, Guid DocumentId);

public sealed record JobResponse(
    Guid Id,
    string Type,
    Guid DocumentId,
    string Status,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt)
{
    public static JobResponse From(BackgroundJob job) =>
        new(
            job.Id,
            job.Type.ToString(),
            job.DocumentId,
            job.Status.ToString(),
            job.ErrorMessage,
            job.CreatedAt,
            job.CompletedAt);
}
