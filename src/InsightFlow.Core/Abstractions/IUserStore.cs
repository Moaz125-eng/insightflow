using InsightFlow.Core.Models;

namespace InsightFlow.Core.Abstractions;

public interface IUserStore
{
    Task<UserAccount?> GetUserAsync(string username, CancellationToken cancellationToken = default);
}
