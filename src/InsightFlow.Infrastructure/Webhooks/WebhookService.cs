using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Webhooks;

public sealed class WebhookService : IWebhookService
{
    public const string JobCompletedEvent = "job.completed";
    public const string JobFailedEvent = "job.failed";

    private readonly SqliteWebhookStore _store;
    private readonly WebhookDispatcher _dispatcher;

    public WebhookService(SqliteWebhookStore store, WebhookDispatcher dispatcher)
    {
        _store = store;
        _dispatcher = dispatcher;
    }

    public async Task<WebhookSubscription> RegisterAsync(string targetUrl, string eventType, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException("Webhook target URL must be absolute.");

        var subscription = new WebhookSubscription
        {
            Id = Guid.NewGuid(),
            TargetUrl = targetUrl.Trim(),
            EventType = eventType.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _store.SaveSubscriptionAsync(subscription, cancellationToken);
        return subscription;
    }

    public Task<IReadOnlyList<WebhookSubscription>> ListAsync(CancellationToken cancellationToken = default) =>
        _store.ListAllAsync(cancellationToken);

    public Task DeactivateAsync(Guid subscriptionId, CancellationToken cancellationToken = default) =>
        _store.DeactivateAsync(subscriptionId, cancellationToken);

    public async Task NotifyJobFinishedAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        var eventType = job.Status == BackgroundJobStatus.Completed ? JobCompletedEvent : JobFailedEvent;
        var payload = new
        {
            jobId = job.Id,
            documentId = job.DocumentId,
            type = job.Type.ToString(),
            status = job.Status.ToString(),
            error = job.ErrorMessage,
            completedAt = job.CompletedAt
        };

        await _dispatcher.DispatchAsync(eventType, payload, cancellationToken);
    }
}
