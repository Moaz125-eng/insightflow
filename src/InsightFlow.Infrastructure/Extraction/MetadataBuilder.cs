using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Extraction;

public sealed class MetadataBuilder
{
    public DocumentMetadata Build(string fileName, string format, string fullText, int pageCount)
    {
        var words = CountWords(fullText);
        return new DocumentMetadata
        {
            Title = Path.GetFileNameWithoutExtension(fileName),
            SourceFormat = format,
            PageCount = pageCount,
            WordCount = words,
            CharacterCount = fullText.Length,
            DetectedLanguages = DetectLanguages(fullText)
        };
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static IReadOnlyList<string> DetectLanguages(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new[] { "unknown" };

        var hasCyrillic = text.Any(c => c is >= '\u0400' and <= '\u04FF');
        var hasArabic = text.Any(c => c is >= '\u0600' and <= '\u06FF');

        if (hasArabic)
            return new[] { "ar", "en" };

        if (hasCyrillic)
            return new[] { "ru", "en" };

        return new[] { "en" };
    }
}
