using FluentAssertions;
using InsightFlow.Core.Configuration;
using InsightFlow.Core.Models;
using InsightFlow.Infrastructure.Audit;
using InsightFlow.Infrastructure.Persistence;
using Xunit;

namespace InsightFlow.Tests;

public sealed class AuditServiceTests
{
    [Fact]
    public async Task Record_and_list_recent_audit_entries()
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

        var service = new AuditService(new SqliteAuditStore(factory));
        await service.RecordAsync(new AuditEntry
        {
            Id = Guid.NewGuid(),
            Actor = "admin",
            Action = "create",
            Resource = "/api/documents",
            Method = "POST",
            StatusCode = 201,
            OccurredAt = DateTimeOffset.UtcNow
        });

        var entries = await service.ListRecentAsync(10);
        entries.Should().ContainSingle(e => e.Actor == "admin" && e.Action == "create");
    }
}
