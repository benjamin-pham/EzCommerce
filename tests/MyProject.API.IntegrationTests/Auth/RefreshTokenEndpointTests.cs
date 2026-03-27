using MyProject.API.IntegrationTests.Infrastructure;

namespace MyProject.API.IntegrationTests.Auth;

public sealed class RefreshTokenEndpointTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    private const string RegisterEndpoint    = "/api/auth/register";
    private const string LoginEndpoint       = "/api/auth/login";
    private const string RefreshTokenEndpoint = "/api/auth/refresh-token";

    private async Task<string> RegisterAndLoginAsync(string username)
    {
        await Client.PostAsJsonAsync(RegisterEndpoint, new
        {
            firstName = "Nguyen",
            lastName  = "Van A",
            username,
            password  = "Secret123"
        });

        var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new
        {
            username,
            password = "Secret123"
        });

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        return loginBody!.RefreshToken;
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_Returns200WithNewTokens()
    {
        var refreshToken = await RegisterAndLoginAsync("refreshuser1");

        var response = await Client.PostAsJsonAsync(RefreshTokenEndpoint, new
        {
            refreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
        body.ExpiresIn.Should().Be(86400);
        body.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_IssuesNewRefreshToken()
    {
        var oldRefreshToken = await RegisterAndLoginAsync("refreshuser2");

        var response = await Client.PostAsJsonAsync(RefreshTokenEndpoint, new
        {
            refreshToken = oldRefreshToken
        });

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();

        body!.RefreshToken.Should().NotBe(oldRefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_Returns401()
    {
        var response = await Client.PostAsJsonAsync(RefreshTokenEndpoint, new
        {
            refreshToken = "this-is-not-a-valid-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithUsedToken_Returns401()
    {
        var oldRefreshToken = await RegisterAndLoginAsync("refreshuser3");

        // Use the token once
        await Client.PostAsJsonAsync(RefreshTokenEndpoint, new { refreshToken = oldRefreshToken });

        // Try to reuse the old token
        var response = await Client.PostAsJsonAsync(RefreshTokenEndpoint, new
        {
            refreshToken = oldRefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record TokenResponse(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn,
        string TokenType);
}
