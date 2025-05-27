using InsightFlow.Core.Configuration;
using Tesseract;

namespace InsightFlow.Infrastructure.Extraction;

public sealed class OcrService
{
    private readonly string _tessDataPath;

    public OcrService(AppSettings settings)
    {
        _tessDataPath = settings.OcrTessdataPath;
    }

    public string ExtractFromImageBytes(byte[] imageBytes)
    {
        if (!Directory.Exists(_tessDataPath))
            return string.Empty;

        try
        {
            using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);
            using var pix = Pix.LoadFromMemory(imageBytes);
            using var page = engine.Process(pix);
            return page.GetText()?.Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public string SupplementSparseText(string extracted, byte[]? previewImage)
    {
        if (!string.IsNullOrWhiteSpace(extracted) && extracted.Length > 80)
            return extracted;

        if (previewImage is null || previewImage.Length == 0)
            return extracted;

        var ocrText = ExtractFromImageBytes(previewImage);
        if (string.IsNullOrWhiteSpace(ocrText))
            return extracted;

        return string.IsNullOrWhiteSpace(extracted)
            ? ocrText
            : $"{extracted}\n{ocrText}";
    }
}
