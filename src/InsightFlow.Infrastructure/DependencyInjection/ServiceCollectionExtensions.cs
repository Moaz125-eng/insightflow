using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Configuration;
using InsightFlow.Core.Services;
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
        return services;
    }
}
