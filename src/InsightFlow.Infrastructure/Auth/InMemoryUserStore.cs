using System.Security.Cryptography;
using System.Text;
using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;

namespace InsightFlow.Infrastructure.Auth;

public sealed class InMemoryUserStore : IUserStore
{
    private readonly Dictionary<string, UserAccount> _users;

    public InMemoryUserStore()
    {
        _users = new Dictionary<string, UserAccount>(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = new UserAccount
            {
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                Roles = new[] { "Admin", "Analyst" }
            },
            ["analyst"] = new UserAccount
            {
                Username = "analyst",
                PasswordHash = HashPassword("analyst123"),
                Roles = new[] { "Analyst" }
            },
            ["viewer"] = new UserAccount
            {
                Username = "viewer",
                PasswordHash = HashPassword("viewer123"),
                Roles = new[] { "Viewer" }
            }
        };
    }

    public Task<UserAccount?> GetUserAsync(string username, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(username, out var user);
        return Task.FromResult(user);
    }

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}
