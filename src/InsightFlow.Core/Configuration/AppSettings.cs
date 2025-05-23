namespace InsightFlow.Core.Configuration;

public sealed class AppSettings
{
    public required string StorageRoot { get; init; }
    public required string DatabasePath { get; init; }
    public required string OnnxModelPath { get; init; }
    public required string OcrTessdataPath { get; init; }
}
