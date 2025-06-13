using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Configuration;
using InsightFlow.Infrastructure.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace InsightFlow.Infrastructure.DependencyInjection;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddInsightFlowAuth(this IServiceCollection services, JwtSettings jwtSettings)
    {
        services.AddSingleton(jwtSettings);
        services.AddSingleton<IUserStore, InMemoryUserStore>();
        services.AddSingleton<JwtTokenService>();
        services.AddSingleton<IAuthService, AuthService>();
        return services;
    }
}
