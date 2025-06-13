using FluentAssertions;
using InsightFlow.Core.Configuration;
using InsightFlow.Infrastructure.Auth;
using Xunit;

namespace InsightFlow.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task Login_returns_token_for_valid_admin_credentials()
    {
        var jwt = new JwtSettings
        {
            Secret = "replace-with-32-char-minimum-secret-key",
            Issuer = "InsightFlow",
            Audience = "InsightFlowClients"
        };

        var store = new InMemoryUserStore();
        var tokens = new JwtTokenService(jwt);
        var auth = new AuthService(store, tokens);

        var result = await auth.LoginAsync("admin", "admin123");
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Login_rejects_invalid_password()
    {
        var jwt = new JwtSettings
        {
            Secret = "replace-with-32-char-minimum-secret-key",
            Issuer = "InsightFlow",
            Audience = "InsightFlowClients"
        };

        var auth = new AuthService(new InMemoryUserStore(), new JwtTokenService(jwt));
        var result = await auth.LoginAsync("admin", "wrong-password");
        result.Should().BeNull();
    }
}
