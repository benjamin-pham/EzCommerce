namespace MyProject.Application.Abstractions.Authentication;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId);
    string GenerateRefreshToken();
    string HashToken(string token);
}
