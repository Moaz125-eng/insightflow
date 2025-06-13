using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InsightFlow.Core.Configuration;
using InsightFlow.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace InsightFlow.Infrastructure.Auth;

public sealed class JwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(JwtSettings settings)
    {
        _settings = settings;
    }

    public AuthTokenResult CreateToken(UserAccount user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTimeOffset.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username)
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        return new AuthTokenResult
        {
            AccessToken = handler.WriteToken(token),
            ExpiresAt = expires,
            Roles = user.Roles
        };
    }
}
