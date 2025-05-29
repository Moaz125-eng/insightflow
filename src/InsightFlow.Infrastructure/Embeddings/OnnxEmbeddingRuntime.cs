using System.Security.Cryptography;
using System.Text;
using InsightFlow.Core.Configuration;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace InsightFlow.Infrastructure.Embeddings;

public sealed class OnnxEmbeddingRuntime : IDisposable
{
    private readonly InferenceSession? _session;

    public string ModelVersion { get; }
    private const int VectorSize = 384;

    public OnnxEmbeddingRuntime(AppSettings settings)
    {
        if (File.Exists(settings.OnnxModelPath))
        {
            _session = new InferenceSession(settings.OnnxModelPath);
            ModelVersion = Path.GetFileName(settings.OnnxModelPath);
        }
        else
        {
            _session = null;
            ModelVersion = "hash-fallback-v1";
        }
    }

    public float[] Encode(string text)
    {
        if (_session is not null)
            return RunOnnx(text);

        return HashFallbackEmbedding(text);
    }

    public IReadOnlyList<float[]> EncodeBatch(IReadOnlyList<string> texts)
    {
        return texts.Select(Encode).ToList();
    }

    private float[] RunOnnx(string text)
    {
        var inputName = _session!.InputMetadata.Keys.First();
        var dims = _session.InputMetadata[inputName].Dimensions.ToArray();
        if (dims.Length == 0 || dims[0] <= 0)
            dims[0] = 1;

        var tensor = new DenseTensor<float>(dims);
        var seed = Math.Abs(text.GetHashCode(StringComparison.Ordinal)) % 997;
        tensor[0] = seed / 1000f;

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(inputName, tensor)
        };

        using var results = _session.Run(inputs);
        var output = results.First().AsEnumerable<float>().ToArray();
        return Normalize(output.Length >= VectorSize ? output.Take(VectorSize).ToArray() : Pad(output));
    }

    private static float[] HashFallbackEmbedding(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        var vector = new float[VectorSize];
        for (var i = 0; i < VectorSize; i++)
            vector[i] = bytes[i % bytes.Length] / 255f;

        return Normalize(vector);
    }

    private static float[] Pad(float[] values)
    {
        var output = new float[VectorSize];
        Array.Copy(values, output, Math.Min(values.Length, VectorSize));
        return Normalize(output);
    }

    private static float[] Normalize(float[] vector)
    {
        var magnitude = Math.Sqrt(vector.Sum(v => v * v));
        if (magnitude <= 0)
            return vector;

        return vector.Select(v => (float)(v / magnitude)).ToArray();
    }

    public void Dispose() => _session?.Dispose();
}
