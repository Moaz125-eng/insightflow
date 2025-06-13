using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserStore _userStore;
    private readonly JwtTokenService _tokenService;

    public AuthService(IUserStore userStore, JwtTokenService tokenService)
    {
        _userStore = userStore;
        _tokenService = tokenService;
    }

    public async Task<AuthTokenResult?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userStore.GetUserAsync(username, cancellationToken);
        if (user is null)
            return null;

        var hash = InMemoryUserStore.HashPassword(password);
        if (!string.Equals(user.PasswordHash, hash, StringComparison.Ordinal))
            return null;

        return _tokenService.CreateToken(user);
    }

    public Task<UserAccount?> GetUserAsync(string username, CancellationToken cancellationToken = default) =>
        _userStore.GetUserAsync(username, cancellationToken);
}
