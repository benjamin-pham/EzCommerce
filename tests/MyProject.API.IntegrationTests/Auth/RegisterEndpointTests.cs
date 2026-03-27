using MyProject.API.IntegrationTests.Infrastructure;

namespace MyProject.API.IntegrationTests.Auth;

public sealed class RegisterEndpointTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    private const string Endpoint = "/api/auth/register";

    [Fact]
    public async Task Register_WithValidRequest_Returns201WithUserId()
    {
        var response = await Client.PostAsJsonAsync(Endpoint, new
        {
            firstName = "Nguyen",
            lastName = "Van A",
            username = "nguyenvana1",
            password = "Secret123",
            email = "nguyenvana@example.com",
            phone = "+84901234567",
            birthday = "1995-06-15"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        body!.UserId.Should().NotBeEmpty();
        body.Username.Should().Be("nguyenvana1");
        body.FirstName.Should().Be("Nguyen");
        body.LastName.Should().Be("Van A");
    }

    [Fact]
    public async Task Register_WithMinimalValidRequest_Returns201()
    {
        var response = await Client.PostAsJsonAsync(Endpoint, new
        {
            firstName = "Nguyen",
            lastName = "Van A",
            username = "minuser1",
            password = "Secret123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_WithMissingLastName_Returns400()
    {
        var response = await Client.PostAsJsonAsync(Endpoint, new
        {
            firstName = "Nguyen",
            username = "usertest2",
            password = "Secret123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMissingFirstName_Returns400()
    {
        var response = await Client.PostAsJsonAsync(Endpoint, new
        {
            lastName = "Van A",
            username = "usertest3",
            password = "Secret123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_Returns409()
    {
        var payload = new
        {
            firstName = "Nguyen",
            lastName = "Van A",
            username = "dupuserx1",
            password = "Secret123"
        };

        await Client.PostAsJsonAsync(Endpoint, payload);
        var response = await Client.PostAsJsonAsync(Endpoint, payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithInvalidEmailFormat_Returns400()
    {
        var response = await Client.PostAsJsonAsync(Endpoint, new
        {
            firstName = "Nguyen",
            lastName = "Van A",
            username = "usertest4",
            password = "Secret123",
            email = "not-an-email"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithPasswordMissingUppercase_Returns400()
    {
        var response = await Client.PostAsJsonAsync(Endpoint, new
        {
            firstName = "Nguyen",
            lastName = "Van A",
            username = "usertest5",
            password = "secret123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithPasswordTooShort_Returns400()
    {
        var response = await Client.PostAsJsonAsync(Endpoint, new
        {
            firstName = "Nguyen",
            lastName = "Van A",
            username = "usertest6",
            password = "Ab1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithUsernameTooShort_Returns400()
    {
        var response = await Client.PostAsJsonAsync(Endpoint, new
        {
            firstName = "Nguyen",
            lastName = "Van A",
            username = "ab",
            password = "Secret123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithUsernameContainingSpecialChars_Returns400()
    {
        var response = await Client.PostAsJsonAsync(Endpoint, new
        {
            firstName = "Nguyen",
            lastName = "Van A",
            username = "user@name",
            password = "Secret123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record RegisterResponse(Guid UserId, string Username, string FirstName, string LastName);
}
