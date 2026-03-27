using MyProject.Domain.Entities;

namespace MyProject.Domain.UnitTests.Auth;

public sealed class UserUpdateProfileTests
{
    private static User CreateValidUser() =>
        User.Create("Nguyen", "Van A", "nguyenvana", "hashedpw", null, null, null);

    [Fact]
    public void UpdateProfile_WithValidArguments_UpdatesFields()
    {
        var user = CreateValidUser();
        var birthday = new DateOnly(1995, 6, 15);

        user.UpdateProfile("Tran", "Van B", "tranvanb@example.com", "+84901234567", birthday);

        user.FirstName.Should().Be("Tran");
        user.LastName.Should().Be("Van B");
        user.Email.Should().Be("tranvanb@example.com");
        user.Phone.Should().Be("+84901234567");
        user.Birthday.Should().Be(birthday);
    }

    [Fact]
    public void UpdateProfile_WithNullOptionalFields_ClearsOptionalFields()
    {
        var user = User.Create("Nguyen", "Van A", "nguyenvana", "hashedpw",
            "existing@example.com", "+84901111111", new DateOnly(1990, 1, 1));

        user.UpdateProfile("Nguyen", "Van A", null, null, null);

        user.Email.Should().BeNull();
        user.Phone.Should().BeNull();
        user.Birthday.Should().BeNull();
    }

    [Fact]
    public void UpdateProfile_DoesNotChangeUsername()
    {
        var user = CreateValidUser();

        user.UpdateProfile("New", "Name", null, null, null);

        user.Username.Should().Be("nguyenvana");
    }

    [Fact]
    public void UpdateProfile_DoesNotChangePasswordHash()
    {
        var user = CreateValidUser();

        user.UpdateProfile("New", "Name", null, null, null);

        user.PasswordHash.Should().Be("hashedpw");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProfile_WithEmptyFirstName_ThrowsArgumentException(string firstName)
    {
        var user = CreateValidUser();

        var act = () => user.UpdateProfile(firstName, "Van A", null, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProfile_WithEmptyLastName_ThrowsArgumentException(string lastName)
    {
        var user = CreateValidUser();

        var act = () => user.UpdateProfile("Nguyen", lastName, null, null, null);

        act.Should().Throw<ArgumentException>();
    }
}
