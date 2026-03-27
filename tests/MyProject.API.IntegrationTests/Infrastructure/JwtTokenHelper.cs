using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MyProject.API.IntegrationTests.Infrastructure;

public static class JwtTokenHelper
{
    // Must match the value configured in CustomWebApplicationFactory
    private const string SecretKey = "integration-test-secret-key-32-chars-min!!";
    private const string Issuer    = "MyProject.Test";
    private const string Audience  = "MyProject.Test";

    public static string GenerateToken(Guid userId, int expirationMinutes = 1440)
    {
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
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
            issuer:             Issuer,
            audience:           Audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string BearerToken(Guid userId) =>
        $"Bearer {GenerateToken(userId)}";
}
