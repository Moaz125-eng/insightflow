using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface IWebhookService
{
    Task<WebhookSubscription> RegisterAsync(string targetUrl, string eventType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WebhookSubscription>> ListAsync(CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task NotifyJobFinishedAsync(BackgroundJob job, CancellationToken cancellationToken = default);
}
