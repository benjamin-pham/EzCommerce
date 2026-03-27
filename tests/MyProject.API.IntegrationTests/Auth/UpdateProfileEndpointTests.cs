using System.Net.Http.Headers;
using MyProject.API.IntegrationTests.Infrastructure;

namespace MyProject.API.IntegrationTests.Auth;

public sealed class UpdateProfileEndpointTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    private const string UpdateEndpoint  = "/api/auth/me";
    private const string ProfileEndpoint = "/api/auth/me";
    private const string RegisterEndpoint = "/api/auth/register";
    private const string LoginEndpoint    = "/api/auth/login";

    private async Task<string> RegisterAndLoginAsync(string username)
    {
        await Client.PostAsJsonAsync(RegisterEndpoint, new
        {
            firstName = "Test",
            lastName  = "User",
            username,
            password  = "Secret123"
        });

        var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new
        {
            username,
            password = "Secret123"
        });

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return loginBody!.AccessToken;
    }

    [Fact]
    public async Task UpdateProfile_WithValidData_Returns204()
    {
        var token = await RegisterAndLoginAsync("updateuser1");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PutAsJsonAsync(UpdateEndpoint, new
        {
            firstName = "Nguyen",
            lastName  = "Thi A",
            email     = "updated@example.com",
            phone     = "+84901234567",
            birthday  = "1990-05-20"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateProfile_ClearRequiredField_Returns400()
    {
        var token = await RegisterAndLoginAsync("updateuser2");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PutAsJsonAsync(UpdateEndpoint, new
        {
            firstName = "",
            lastName  = "Thi B"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_InvalidEmailFormat_Returns400()
    {
        var token = await RegisterAndLoginAsync("updateuser3");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PutAsJsonAsync(UpdateEndpoint, new
        {
            firstName = "Test",
            lastName  = "User",
            email     = "not-an-email"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_FutureBirthday_Returns400()
    {
        var token = await RegisterAndLoginAsync("updateuser4");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)).ToString("yyyy-MM-dd");

        var response = await Client.PutAsJsonAsync(UpdateEndpoint, new
        {
            firstName = "Test",
            lastName  = "User",
            birthday  = futureDate
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_WithoutToken_Returns401()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PutAsJsonAsync(UpdateEndpoint, new
        {
            firstName = "Test",
            lastName  = "User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_DoesNotChangeUsername()
    {
        var token = await RegisterAndLoginAsync("updateuser5");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await Client.PutAsJsonAsync(UpdateEndpoint, new
        {
            firstName = "Changed",
            lastName  = "Name"
        });

        var profileResponse = await Client.GetAsync(ProfileEndpoint);
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await profileResponse.Content.ReadFromJsonAsync<ProfileResponse>();
        profile!.Username.Should().Be("updateuser5");
        profile.FirstName.Should().Be("Changed");
        profile.LastName.Should().Be("Name");
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
