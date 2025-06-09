using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface IBackgroundJobQueue
{
    ValueTask EnqueueAsync(BackgroundJob job, CancellationToken cancellationToken = default);
    ValueTask<BackgroundJob?> DequeueAsync(CancellationToken cancellationToken = default);
    Task<BackgroundJob?> GetAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BackgroundJob>> ListAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(BackgroundJob job, CancellationToken cancellationToken = default);
}
