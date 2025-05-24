using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Configuration;

namespace InsightFlow.Infrastructure.Storage;

public sealed class LocalDocumentStorage : IDocumentStorage
{
    private readonly string _root;

    public LocalDocumentStorage(AppSettings settings)
    {
        _root = Path.GetFullPath(settings.StorageRoot);
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(_root, safeName);

        await using var output = File.Create(fullPath);
        await content.CopyToAsync(output, cancellationToken);

        return fullPath;
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(storagePath))
            throw new FileNotFoundException("Stored document not found.", storagePath);

        Stream stream = File.OpenRead(storagePath);
        return Task.FromResult(stream);
    }

    public Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(storagePath))
            File.Delete(storagePath);

        return Task.CompletedTask;
    }
}
