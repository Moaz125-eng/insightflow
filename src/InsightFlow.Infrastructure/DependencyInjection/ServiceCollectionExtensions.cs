using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Configuration;
using InsightFlow.Core.Services;
using InsightFlow.Infrastructure.Embeddings;
using InsightFlow.Infrastructure.Search;
using InsightFlow.Infrastructure.Analytics;
using InsightFlow.Infrastructure.Export;
using InsightFlow.Infrastructure.Background;
using InsightFlow.Infrastructure.Qa;
using InsightFlow.Infrastructure.Summarization;
using InsightFlow.Infrastructure.Extraction;
using InsightFlow.Infrastructure.Persistence;
using InsightFlow.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace InsightFlow.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInsightFlowInfrastructure(
        this IServiceCollection services,
        AppSettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton<IDocumentRepository, InMemoryDocumentRepository>();
        services.AddSingleton<IDocumentStorage, LocalDocumentStorage>();
        services.AddSingleton<IDocumentUploadService, DocumentUploadService>();
        services.AddSingleton<OcrService>();
        services.AddSingleton<ChunkingService>();
        services.AddSingleton<ITextExtractionService, TextExtractionService>();
        services.AddSingleton<EmbeddingCache>(sp =>
        {
            var settings = sp.GetRequiredService<AppSettings>();
            var root = Path.GetDirectoryName(settings.DatabasePath) ?? "./data";
            return new EmbeddingCache(root);
        });
        services.AddSingleton<OnnxEmbeddingRuntime>();
        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        services.AddSingleton<IVectorSearchService, VectorSearchService>();
        services.AddSingleton<ISummarizationService, SummarizationService>();
        services.AddSingleton<RagRetriever>();
        services.AddSingleton<IQuestionAnsweringService, QuestionAnsweringService>();
        services.AddSingleton<AnalyticsStore>();
        services.AddSingleton<IAnalyticsService, AnalyticsService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<IBackgroundJobQueue, ChannelJobQueue>();
        services.AddSingleton<BackgroundJobProcessor>();
        services.AddHostedService<BackgroundWorker>();
        return services;
    }
}
