using MyProject.Domain.Entities;

namespace MyProject.Domain.UnitTests.Auth;

public sealed class UserCreateTests
{
    [Fact]
    public void Create_WithValidArguments_ReturnsUser()
    {
        var user = User.Create("Nguyen", "Van A", "nguyenvana", "hashedpw", null, null, null);

        user.FirstName.Should().Be("Nguyen");
        user.LastName.Should().Be("Van A");
        user.Username.Should().Be("nguyenvana");
        user.PasswordHash.Should().Be("hashedpw");
        user.Id.Should().NotBeEmpty();
        user.Email.Should().BeNull();
        user.Phone.Should().BeNull();
        user.Birthday.Should().BeNull();
        user.HashedRefreshToken.Should().BeNull();
        user.RefreshTokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithOptionalFields_SetsThemCorrectly()
    {
        var birthday = new DateOnly(1995, 6, 15);

        var user = User.Create(
            "Nguyen", "Van A", "nguyenvana", "hashedpw",
            "nguyenvana@example.com", "+84901234567", birthday);

        user.Email.Should().Be("nguyenvana@example.com");
        user.Phone.Should().Be("+84901234567");
        user.Birthday.Should().Be(birthday);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyFirstName_ThrowsArgumentException(string firstName)
    {
        var act = () => User.Create(firstName, "Van A", "nguyenvana", "hashedpw", null, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyLastName_ThrowsArgumentException(string lastName)
    {
        var act = () => User.Create("Nguyen", lastName, "nguyenvana", "hashedpw", null, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyUsername_ThrowsArgumentException(string username)
    {
        var act = () => User.Create("Nguyen", "Van A", username, "hashedpw", null, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyPasswordHash_ThrowsArgumentException(string passwordHash)
    {
        var act = () => User.Create("Nguyen", "Van A", "nguyenvana", passwordHash, null, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EachCallProducesDifferentId()
    {
        var user1 = User.Create("A", "B", "usernameab1", "hash", null, null, null);
        var user2 = User.Create("A", "B", "usernameab2", "hash", null, null, null);

        user1.Id.Should().NotBe(user2.Id);
    }
}
