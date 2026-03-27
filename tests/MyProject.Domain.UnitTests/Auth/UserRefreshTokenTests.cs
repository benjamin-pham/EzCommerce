using MyProject.Domain.Entities;

namespace MyProject.Domain.UnitTests.Auth;

public sealed class UserRefreshTokenTests
{
    private static User CreateValidUser() =>
        User.Create("Nguyen", "Van A", "nguyenvana", "hashedpw", null, null, null);

    [Fact]
    public void SetRefreshToken_WithValidArguments_SetsTokenAndExpiry()
    {
        var user = CreateValidUser();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        user.SetRefreshToken("hashed-token-value", expiresAt);

        user.HashedRefreshToken.Should().Be("hashed-token-value");
        user.RefreshTokenExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void SetRefreshToken_CalledTwice_OverwritesPreviousToken()
    {
        var user = CreateValidUser();
        user.SetRefreshToken("old-token", DateTime.UtcNow.AddDays(7));

        var newExpiry = DateTime.UtcNow.AddDays(7);
        user.SetRefreshToken("new-token", newExpiry);

        user.HashedRefreshToken.Should().Be("new-token");
        user.RefreshTokenExpiresAt.Should().Be(newExpiry);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetRefreshToken_WithEmptyHashedToken_ThrowsArgumentException(string hashedToken)
    {
        var user = CreateValidUser();

        var act = () => user.SetRefreshToken(hashedToken, DateTime.UtcNow.AddDays(7));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RevokeRefreshToken_ClearsHashedTokenAndExpiry()
    {
        var user = CreateValidUser();
        user.SetRefreshToken("hashed-token-value", DateTime.UtcNow.AddDays(7));

        user.RevokeRefreshToken();

        user.HashedRefreshToken.Should().BeNull();
        user.RefreshTokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public void RevokeRefreshToken_WhenAlreadyRevoked_RemainsNull()
    {
        var user = CreateValidUser();

        user.RevokeRefreshToken();

        user.HashedRefreshToken.Should().BeNull();
        user.RefreshTokenExpiresAt.Should().BeNull();
    }
}
