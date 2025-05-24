namespace InsightFlow.Core.Validation;

public static class UploadValidation
{
    public const long MaxFileSizeBytes = 52_428_800;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".txt"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain"
    };

    public static UploadValidationResult Validate(string fileName, string contentType, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return UploadValidationResult.Fail("File name is required.");

        if (sizeBytes <= 0)
            return UploadValidationResult.Fail("File is empty.");

        if (sizeBytes > MaxFileSizeBytes)
            return UploadValidationResult.Fail($"File exceeds maximum size of {MaxFileSizeBytes} bytes.");

        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
            return UploadValidationResult.Fail($"Extension '{extension}' is not supported.");

        if (!string.IsNullOrWhiteSpace(contentType) && !AllowedContentTypes.Contains(contentType))
            return UploadValidationResult.Fail($"Content type '{contentType}' is not supported.");

        return UploadValidationResult.Ok();
    }
}

public sealed class UploadValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }

    public static UploadValidationResult Ok() => new() { IsValid = true };
    public static UploadValidationResult Fail(string message) => new() { IsValid = false, ErrorMessage = message };
}
