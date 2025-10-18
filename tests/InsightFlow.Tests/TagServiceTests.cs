using FluentAssertions;
using InsightFlow.Core.Configuration;
using InsightFlow.Core.Models;
using InsightFlow.Infrastructure.Persistence;
using InsightFlow.Infrastructure.Tags;
using Xunit;

namespace InsightFlow.Tests;

public sealed class TagServiceTests
{
    [Fact]
    public async Task Create_assign_and_list_tags_for_document()
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

        var documents = new SqliteDocumentRepository(factory);
        var tags = new SqliteTagRepository(factory);
        var service = new TagService(tags, documents);

        var documentId = Guid.NewGuid();
        await documents.SaveAsync(new DocumentRecord
        {
            Id = documentId,
            FileName = "finance.pdf",
            ContentType = "application/pdf",
            SizeBytes = 100,
            StoragePath = "/tmp/finance.pdf",
            Status = DocumentStatus.Pending,
            UploadedAt = DateTimeOffset.UtcNow
        });

        var tag = await service.CreateTagAsync("finance", "#10B981");
        await service.AssignTagAsync(documentId, tag.Id);

        var tagged = await service.GetDocumentTagsAsync(documentId);
        tagged!.Tags.Should().ContainSingle(t => t.Name == "finance");
    }
}
