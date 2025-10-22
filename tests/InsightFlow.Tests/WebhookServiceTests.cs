using FluentAssertions;
using InsightFlow.Core.Configuration;
using InsightFlow.Infrastructure.Persistence;
using InsightFlow.Infrastructure.Webhooks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InsightFlow.Tests;

public sealed class WebhookServiceTests
{
    [Fact]
    public async Task Register_lists_active_webhook_subscription()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var settings = new AppSettings
        {
            StorageRoot = Path.Combine(root, "uploads"),
            DatabasePath = Path.Combine(root, "insightflow.db"),
            OnnxModelPath = Path.Combine(root, "embedding.onnx"),
            OcrTessdataPath = Path.Combine(root, "tessdata")
        };

        var factory = new SqliteConnectionFactory(settings);
        new DatabaseInitializer(factory).Initialize();

        var services = new ServiceCollection();
        services.AddHttpClient("webhooks");
        var provider = services.BuildServiceProvider();

        var store = new SqliteWebhookStore(factory);
        var dispatcher = new WebhookDispatcher(provider.GetRequiredService<IHttpClientFactory>(), store);
        var service = new WebhookService(store, dispatcher);

        var subscription = await service.RegisterAsync("https://example.com/hooks/insightflow", WebhookService.JobCompletedEvent);
        var listed = await service.ListAsync();

        listed.Should().ContainSingle(w => w.Id == subscription.Id && w.IsActive);
    }
}
