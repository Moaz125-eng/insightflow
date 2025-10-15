using FluentAssertions;
using InsightFlow.Core.Configuration;
using InsightFlow.Core.Models;
using InsightFlow.Infrastructure.Persistence;
using Xunit;

namespace InsightFlow.Tests;

public sealed class SqliteDocumentRepositoryTests
{
    [Fact]
    public async Task Save_and_load_document_roundtrip()
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
        var repository = new SqliteDocumentRepository(factory);

        var record = new DocumentRecord
        {
            Id = Guid.NewGuid(),
            FileName = "report.pdf",
            ContentType = "application/pdf",
            SizeBytes = 4096,
            StoragePath = "/tmp/report.pdf",
            Status = DocumentStatus.Pending,
            UploadedAt = DateTimeOffset.UtcNow
        };

        await repository.SaveAsync(record);
        var loaded = await repository.GetByIdAsync(record.Id);

        loaded.Should().NotBeNull();
        loaded!.FileName.Should().Be("report.pdf");
        (await repository.ListAsync()).Should().ContainSingle(d => d.Id == record.Id);
    }

    [Fact]
    public async Task Delete_removes_document_row()
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
        var repository = new SqliteDocumentRepository(factory);
        var id = Guid.NewGuid();

        await repository.SaveAsync(new DocumentRecord
        {
            Id = id,
            FileName = "notes.txt",
            ContentType = "text/plain",
            SizeBytes = 12,
            StoragePath = "/tmp/notes.txt",
            Status = DocumentStatus.Indexed,
            UploadedAt = DateTimeOffset.UtcNow
        });

        await repository.DeleteAsync(id);
        (await repository.GetByIdAsync(id)).Should().BeNull();
    }
}
