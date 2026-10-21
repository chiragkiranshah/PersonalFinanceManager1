using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace PersonalFinanceManager.Identity.Api;

public sealed record RegisterRequest(string Email, string UserName, string FirstName, string LastName, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(Guid UserId, string Email, string AccessToken, DateTime ExpiresAtUtc, string TokenType = "Bearer");
public sealed record UserProfileResponse(Guid UserId, string Email, string UserName, string FirstName, string LastName);

public sealed record AppUser(Guid Id, string Email, string UserName, string FirstName, string LastName, string PasswordHash);

public interface IUserStore
{
    Task AddAsync(AppUser user, CancellationToken cancellationToken = default);
    Task<AppUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<AppUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class InMemoryUserStore : IUserStore
{
    private readonly Dictionary<string, AppUser> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);

    public Task AddAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        _usersByEmail[user.Email] = user;
        return Task.CompletedTask;
    }

    public Task<AppUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _usersByEmail.TryGetValue(email, out var user);
        return Task.FromResult(user);
    }

    public Task<AppUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = _usersByEmail.Values.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(user);
    }
}

public interface IJwtTokenService
{
    string CreateToken(AppUser user);
}

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string CreateToken(AppUser user)
    {
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "development-secret-key-please-change");
        var securityKey = new SymmetricSecurityKey(key);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(2);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "PersonalFinanceManager.Identity",
            audience: configuration["Jwt:Audience"] ?? "PersonalFinanceManager.Api",
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string password, string hash)
    {
        return string.Equals(Hash(password), hash, StringComparison.OrdinalIgnoreCase);
    }
}
