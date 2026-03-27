using System.Net.Http.Headers;
using MyProject.API.IntegrationTests.Infrastructure;

namespace MyProject.API.IntegrationTests.Auth;

public sealed class GetProfileEndpointTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    private const string ProfileEndpoint  = "/api/auth/me";
    private const string RegisterEndpoint = "/api/auth/register";
    private const string LoginEndpoint    = "/api/auth/login";

    [Fact]
    public async Task GetProfile_WithValidToken_Returns200WithAllFields()
    {
        // Register a user
        var registerResponse = await Client.PostAsJsonAsync(RegisterEndpoint, new
        {
            firstName = "Tran",
            lastName  = "Thi B",
            username  = "profileuser1",
            password  = "Secret123",
            email     = "tranb@example.com",
            phone     = "+84901234567",
            birthday  = "1990-01-15"
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Login to obtain a real access token
        var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new
        {
            username = "profileuser1",
            password = "Secret123"
        });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

        // GET /api/auth/me
        var response = await Client.GetAsync(ProfileEndpoint);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ProfileResponse>();
        body!.FirstName.Should().Be("Tran");
        body.LastName.Should().Be("Thi B");
        body.Username.Should().Be("profileuser1");
        body.Email.Should().Be("tranb@example.com");
        body.Phone.Should().Be("+84901234567");
        body.UserId.Should().NotBeEmpty();
        body.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_Returns401()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.GetAsync(ProfileEndpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_WithApiKeyOnly_Returns401()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        Client.DefaultRequestHeaders.Add("X-Api-Key", "test-api-key");

        var response = await Client.GetAsync(ProfileEndpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        Client.DefaultRequestHeaders.Remove("X-Api-Key");
    }

    [Fact]
    public async Task GetProfile_DoesNotReturnPassword()
    {
        await Client.PostAsJsonAsync(RegisterEndpoint, new
        {
            firstName = "Le",
            lastName  = "Van C",
            username  = "profileuser2",
            password  = "Secret123"
        });

        var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new
        {
            username = "profileuser2",
            password = "Secret123"
        });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

        var response = await Client.GetAsync(ProfileEndpoint);
        var raw      = await response.Content.ReadAsStringAsync();

        raw.Should().NotContain("passwordHash", because: "password must never be returned (FR-010)");
        raw.Should().NotContain("password_hash", because: "password must never be returned (FR-010)");
    }

    private sealed record LoginResponse(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn,
        string TokenType);

    private sealed record ProfileResponse(
        Guid UserId,
        string FirstName,
        string LastName,
        string Username,
        string? Email,
        string? Phone,
        DateOnly? Birthday,
        DateTime CreatedAt);
}
