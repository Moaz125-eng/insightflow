using System.Globalization;
using CsvHelper;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Export;

public sealed class CsvAnalyticsExporter
{
    public byte[] Build(AnalyticsSnapshot snapshot)
    {
        using var memory = new MemoryStream();
        using var writer = new StreamWriter(memory);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteField("metric");
        csv.WriteField("value");
        csv.NextRecord();
        csv.WriteField("indexed_documents");
        csv.WriteField(snapshot.IndexedDocuments);
        csv.NextRecord();
        csv.WriteField("total_searches");
        csv.WriteField(snapshot.TotalSearches);
        csv.NextRecord();
        csv.WriteField("total_embeddings");
        csv.WriteField(snapshot.TotalEmbeddings);
        csv.NextRecord();
        csv.WriteField("avg_embedding_dimensions");
        csv.WriteField(snapshot.AverageEmbeddingDimensions);
        csv.NextRecord();
        csv.WriteField("generated_at");
        csv.WriteField(snapshot.GeneratedAt.ToString("O"));
        csv.NextRecord();

        csv.NextRecord();
        csv.WriteField("date");
        csv.WriteField("search_count");
        csv.NextRecord();

        foreach (var metric in snapshot.SearchFrequency)
        {
            csv.WriteField(metric.Date.ToString("yyyy-MM-dd"));
            csv.WriteField(metric.Count);
            csv.NextRecord();
        }

        writer.Flush();
        return memory.ToArray();
    }
}
