using InsightFlow.Core.Models;
using InsightFlow.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;

namespace InsightFlow.Infrastructure.Webhooks;

public sealed class SqliteWebhookStore
{
    private readonly SqliteConnectionFactory _factory;

    public SqliteWebhookStore(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task SaveSubscriptionAsync(WebhookSubscription subscription, CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
"""
INSERT INTO webhooks (id, target_url, event_type, is_active, created_at)
VALUES ($id, $target_url, $event_type, $is_active, $created_at);
""";
        command.Parameters.AddWithValue("$id", subscription.Id.ToString());
        command.Parameters.AddWithValue("$target_url", subscription.TargetUrl);
        command.Parameters.AddWithValue("$event_type", subscription.EventType);
        command.Parameters.AddWithValue("$is_active", subscription.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("$created_at", subscription.CreatedAt.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WebhookSubscription>> ListActiveByEventAsync(string eventType, CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, target_url, event_type, is_active, created_at FROM webhooks WHERE event_type = $event_type AND is_active = 1";
        command.Parameters.AddWithValue("$event_type", eventType);

        var items = new List<WebhookSubscription>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new WebhookSubscription
            {
                Id = Guid.Parse(reader.GetString(0)),
                TargetUrl = reader.GetString(1),
                EventType = reader.GetString(2),
                IsActive = reader.GetInt32(3) == 1,
                CreatedAt = DateTimeOffset.Parse(reader.GetString(4))
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<WebhookSubscription>> ListAllAsync(CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, target_url, event_type, is_active, created_at FROM webhooks ORDER BY created_at DESC";

        var items = new List<WebhookSubscription>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new WebhookSubscription
            {
                Id = Guid.Parse(reader.GetString(0)),
                TargetUrl = reader.GetString(1),
                EventType = reader.GetString(2),
                IsActive = reader.GetInt32(3) == 1,
                CreatedAt = DateTimeOffset.Parse(reader.GetString(4))
            });
        }

        return items;
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE webhooks SET is_active = 0 WHERE id = $id";
        command.Parameters.AddWithValue("$id", id.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveDeliveryAsync(WebhookDelivery delivery, CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
"""
INSERT INTO webhook_deliveries (subscription_id, event_type, payload_json, status_code, succeeded, delivered_at)
VALUES ($subscription_id, $event_type, $payload_json, $status_code, $succeeded, $delivered_at);
""";
        command.Parameters.AddWithValue("$subscription_id", delivery.SubscriptionId.ToString());
        command.Parameters.AddWithValue("$event_type", delivery.EventType);
        command.Parameters.AddWithValue("$payload_json", delivery.PayloadJson);
        command.Parameters.AddWithValue("$status_code", delivery.StatusCode);
        command.Parameters.AddWithValue("$succeeded", delivery.Succeeded ? 1 : 0);
        command.Parameters.AddWithValue("$delivered_at", delivery.DeliveredAt.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
