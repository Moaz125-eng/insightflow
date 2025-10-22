using System.Net.Http.Json;
using System.Text.Json;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Webhooks;

public sealed class WebhookDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly SqliteWebhookStore _store;

    public WebhookDispatcher(IHttpClientFactory httpClientFactory, SqliteWebhookStore store)
    {
        _httpClient = httpClientFactory.CreateClient("webhooks");
        _store = store;
    }

    public async Task DispatchAsync(string eventType, object payload, CancellationToken cancellationToken)
    {
        var subscriptions = await _store.ListActiveByEventAsync(eventType, cancellationToken);
        var json = JsonSerializer.Serialize(payload);

        foreach (var subscription in subscriptions)
        {
            var statusCode = 0;
            var succeeded = false;

            try
            {
                using var response = await _httpClient.PostAsJsonAsync(subscription.TargetUrl, payload, cancellationToken);
                statusCode = (int)response.StatusCode;
                succeeded = response.IsSuccessStatusCode;
            }
            catch
            {
                statusCode = 0;
                succeeded = false;
            }

            await _store.SaveDeliveryAsync(new WebhookDelivery
            {
                SubscriptionId = subscription.Id,
                EventType = eventType,
                PayloadJson = json,
                StatusCode = statusCode,
                Succeeded = succeeded,
                DeliveredAt = DateTimeOffset.UtcNow
            }, cancellationToken);
        }
    }
}
