using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface IAuthService
{
    Task<AuthTokenResult?> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetUserAsync(string username, CancellationToken cancellationToken = default);
}
