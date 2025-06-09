using System.Collections.Concurrent;
using System.Threading.Channels;
using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Background;

public sealed class ChannelJobQueue : IBackgroundJobQueue
{
    private readonly Channel<BackgroundJob> _channel = Channel.CreateUnbounded<BackgroundJob>();
    private readonly ConcurrentDictionary<Guid, BackgroundJob> _jobs = new();

    public async ValueTask EnqueueAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        _jobs[job.Id] = job;
        await _channel.Writer.WriteAsync(job, cancellationToken);
    }

    public async ValueTask<BackgroundJob?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        var job = await _channel.Reader.ReadAsync(cancellationToken);
        return job;
    }

    public Task<BackgroundJob?> GetAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        _jobs.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    public Task<IReadOnlyList<BackgroundJob>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<BackgroundJob> jobs = _jobs.Values.OrderByDescending(j => j.CreatedAt).ToList();
        return Task.FromResult(jobs);
    }

    public Task UpdateAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        _jobs[job.Id] = job;
        return Task.CompletedTask;
    }
}
