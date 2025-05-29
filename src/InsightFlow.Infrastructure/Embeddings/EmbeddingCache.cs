using System.Collections.Concurrent;
using System.Text.Json;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Embeddings;

public sealed class EmbeddingCache
{
    private readonly ConcurrentDictionary<string, EmbeddingVector> _memory = new();
    private readonly string _diskRoot;

    public EmbeddingCache(string diskRoot)
    {
        _diskRoot = Path.Combine(diskRoot, "embedding-cache");
        Directory.CreateDirectory(_diskRoot);
    }

    public bool TryGet(Guid documentId, int chunkIndex, out EmbeddingVector? vector)
    {
        var key = BuildKey(documentId, chunkIndex);
        if (_memory.TryGetValue(key, out vector))
            return true;

        var path = GetDiskPath(key);
        if (!File.Exists(path))
        {
            vector = null;
            return false;
        }

        var json = File.ReadAllText(path);
        vector = JsonSerializer.Deserialize<EmbeddingVector>(json);
        if (vector is not null)
            _memory[key] = vector;

        return vector is not null;
    }

    public void Set(EmbeddingVector vector)
    {
        var key = BuildKey(vector.DocumentId, vector.ChunkIndex);
        _memory[key] = vector;

        var path = GetDiskPath(key);
        var json = JsonSerializer.Serialize(vector);
        File.WriteAllText(path, json);
    }

    public int CountInMemory() => _memory.Count;

    private static string BuildKey(Guid documentId, int chunkIndex) => $"{documentId:N}:{chunkIndex}";

    private string GetDiskPath(string key) => Path.Combine(_diskRoot, $"{key.Replace(':', '_')}.json");
}
