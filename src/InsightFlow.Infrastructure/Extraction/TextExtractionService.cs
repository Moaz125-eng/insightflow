using System.Collections.Concurrent;
using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Extraction;

public sealed class TextExtractionService : ITextExtractionService
{
    private readonly IDocumentUploadService _uploadService;
    private readonly IDocumentStorage _storage;
    private readonly PdfTextExtractor _pdfExtractor = new();
    private readonly DocxTextExtractor _docxExtractor = new();
    private readonly TxtTextExtractor _txtExtractor = new();
    private readonly OcrService _ocrService;
    private readonly ChunkingService _chunking = new();
    private readonly MetadataBuilder _metadataBuilder = new();
    private readonly ConcurrentDictionary<Guid, ExtractedDocument> _cache = new();

    public TextExtractionService(
        IDocumentUploadService uploadService,
        IDocumentStorage storage,
        OcrService ocrService)
    {
        _uploadService = uploadService;
        _storage = storage;
        _ocrService = ocrService;
    }

    public async Task<ExtractedDocument> ExtractAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var record = await _uploadService.GetAsync(documentId, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        await using var stream = await _storage.OpenReadAsync(record.StoragePath, cancellationToken);
        var extension = Path.GetExtension(record.FileName).ToLowerInvariant();

        var (text, pageCount, format) = extension switch
        {
            ".pdf" => ExtractPdf(stream),
            ".docx" => ExtractDocx(stream),
            ".txt" => await ExtractTxt(stream, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported format: {extension}")
        };

        var enriched = _ocrService.SupplementSparseText(text, null);
        var metadata = _metadataBuilder.Build(record.FileName, format, enriched, pageCount);
        var chunks = _chunking.Chunk(enriched);

        var extracted = new ExtractedDocument
        {
            DocumentId = documentId,
            FullText = enriched,
            Metadata = metadata,
            Chunks = chunks,
            ExtractedAt = DateTimeOffset.UtcNow
        };

        _cache[documentId] = extracted;
        return extracted;
    }

    public Task<ExtractedDocument?> GetExtractedAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(documentId, out var value);
        return Task.FromResult(value);
    }

    private (string Text, int PageCount, string Format) ExtractPdf(Stream stream)
    {
        var (text, pages) = _pdfExtractor.Extract(stream);
        return (text, pages, "pdf");
    }

    private (string Text, int PageCount, string Format) ExtractDocx(Stream stream)
    {
        var (text, pages) = _docxExtractor.Extract(stream);
        return (text, pages, "docx");
    }

    private async Task<(string Text, int PageCount, string Format)> ExtractTxt(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var (text, pages) = await _txtExtractor.ExtractAsync(stream, cancellationToken);
        return (text, pages, "txt");
    }
}
