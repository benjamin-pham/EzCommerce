using MyProject.API.IntegrationTests.Infrastructure;

namespace MyProject.API.IntegrationTests.Auth;

public sealed class LoginEndpointTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    private const string LoginEndpoint    = "/api/auth/login";
    private const string RegisterEndpoint = "/api/auth/register";

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithTokens()
    {
        await Client.PostAsJsonAsync(RegisterEndpoint, new
        {
            firstName = "Nguyen",
            lastName  = "Van A",
            username  = "loginuser1",
            password  = "Secret123"
        });

        var response = await Client.PostAsJsonAsync(LoginEndpoint, new
        {
            username = "loginuser1",
            password = "Secret123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
        body.ExpiresIn.Should().Be(86400);
        body.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        await Client.PostAsJsonAsync(RegisterEndpoint, new
        {
            firstName = "Nguyen",
            lastName  = "Van A",
            username  = "loginuser2",
            password  = "Secret123"
        });

        var response = await Client.PostAsJsonAsync(LoginEndpoint, new
        {
            username = "loginuser2",
            password = "WrongPassword1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownUsername_Returns401()
    {
        var response = await Client.PostAsJsonAsync(LoginEndpoint, new
        {
            username = "doesnotexist",
            password = "Secret123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithMissingUsername_Returns400()
    {
        var response = await Client.PostAsJsonAsync(LoginEndpoint, new
        {
            password = "Secret123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithMissingPassword_Returns400()
    {
        var response = await Client.PostAsJsonAsync(LoginEndpoint, new
        {
            username = "loginuser3"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record LoginResponse(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn,
        string TokenType);
}
