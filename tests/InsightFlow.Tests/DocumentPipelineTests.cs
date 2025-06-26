using FluentAssertions;
using InsightFlow.Core.Configuration;
using InsightFlow.Core.Services;
using InsightFlow.Infrastructure.Extraction;
using InsightFlow.Infrastructure.Persistence;
using InsightFlow.Infrastructure.Storage;
using Xunit;

namespace InsightFlow.Tests;

public sealed class DocumentPipelineTests
{
    [Fact]
    public async Task Upload_extract_and_chunk_txt_document_end_to_end()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var settings = new AppSettings
        {
            StorageRoot = Path.Combine(root, "uploads"),
            DatabasePath = Path.Combine(root, "insightflow.db"),
            OnnxModelPath = Path.Combine(root, "embedding.onnx"),
            OcrTessdataPath = Path.Combine(root, "tessdata")
        };

        var repository = new InMemoryDocumentRepository();
        var storage = new LocalDocumentStorage(settings);
        var uploadService = new DocumentUploadService(repository, storage);
        var extractionService = new TextExtractionService(
            uploadService,
            storage,
            new OcrService(settings));

        await using var stream = new MemoryStream("InsightFlow enables semantic retrieval and document intelligence."u8.ToArray());
        var uploaded = await uploadService.UploadAsync(stream, "notes.txt", "text/plain", stream.Length);

        var extracted = await extractionService.ExtractAsync(uploaded.Id);
        extracted.Chunks.Should().NotBeEmpty();
        extracted.Metadata.WordCount.Should().BeGreaterThan(3);
        extracted.FullText.Should().Contain("semantic retrieval");
    }
}
