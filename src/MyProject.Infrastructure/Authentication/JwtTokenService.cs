using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyProject.Application.Abstractions.Authentication;

namespace MyProject.Infrastructure.Authentication;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    private readonly string _secretKey = configuration["Authentication:Jwt:SecretKey"]
        ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

    private readonly string _issuer = configuration["Authentication:Jwt:Issuer"]
        ?? throw new InvalidOperationException("JWT Issuer is not configured.");

    private readonly string _audience = configuration["Authentication:Jwt:Audience"]
        ?? throw new InvalidOperationException("JWT Audience is not configured.");

    private readonly int _accessTokenExpirationMinutes =
        int.Parse(configuration["Authentication:Jwt:AccessTokenExpirationMinutes"] ?? "1440");

    public string GenerateAccessToken(Guid userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
