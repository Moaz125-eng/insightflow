using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InsightFlow.Infrastructure.Background;

public sealed class BackgroundWorker : BackgroundService
{
    private readonly IBackgroundJobQueue _queue;
    private readonly BackgroundJobProcessor _processor;
    private readonly ILogger<BackgroundWorker> _logger;

    public BackgroundWorker(
        IBackgroundJobQueue queue,
        BackgroundJobProcessor processor,
        ILogger<BackgroundWorker> logger)
    {
        _queue = queue;
        _processor = processor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var job = await _queue.DequeueAsync(stoppingToken);
            if (job is null)
                continue;

            var running = job with
            {
                Status = BackgroundJobStatus.Running,
                StartedAt = DateTimeOffset.UtcNow
            };
            await _queue.UpdateAsync(running, stoppingToken);

            try
            {
                await _processor.ExecuteAsync(running, stoppingToken);
                var completed = running with
                {
                    Status = BackgroundJobStatus.Completed,
                    CompletedAt = DateTimeOffset.UtcNow
                };
                await _queue.UpdateAsync(completed, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background job {JobId} failed", running.Id);
                var failed = running with
                {
                    Status = BackgroundJobStatus.Failed,
                    ErrorMessage = ex.Message,
                    CompletedAt = DateTimeOffset.UtcNow
                };
                await _queue.UpdateAsync(failed, stoppingToken);
            }
        }
    }
}
