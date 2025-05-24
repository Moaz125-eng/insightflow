using FluentAssertions;
using InsightFlow.Core.Validation;
using Xunit;

namespace InsightFlow.Tests;

public sealed class UploadValidationTests
{
    [Theory]
    [InlineData("report.pdf", "application/pdf", 1024, true)]
    [InlineData("notes.txt", "text/plain", 512, true)]
    [InlineData("brief.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 2048, true)]
    [InlineData("image.png", "image/png", 1024, false)]
    [InlineData("huge.pdf", "application/pdf", UploadValidation.MaxFileSizeBytes + 1, false)]
    [InlineData("", "application/pdf", 100, false)]
    public void Validate_returns_expected_outcome(
        string fileName,
        string contentType,
        long size,
        bool expectedValid)
    {
        var result = UploadValidation.Validate(fileName, contentType, size);
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void Validate_rejects_zero_byte_upload()
    {
        var result = UploadValidation.Validate("empty.txt", "text/plain", 0);
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }
}
