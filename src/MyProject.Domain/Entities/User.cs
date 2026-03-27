using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public sealed class User : BaseEntity
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateOnly? Birthday { get; set; }
    public string? HashedRefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public User() { }

    public static User Create(
        string firstName,
        string lastName,
        string username,
        string passwordHash,
        string? email,
        string? phone,
        DateOnly? birthday)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Username = username,
            PasswordHash = passwordHash,
            Email = email,
            Phone = phone,
            Birthday = birthday
        };
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        string? email,
        string? phone,
        DateOnly? birthday)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Birthday = birthday;
    }

    public void SetRefreshToken(string hashedToken, DateTime expiresAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hashedToken);

        HashedRefreshToken = hashedToken;
        RefreshTokenExpiresAt = expiresAt;
    }

    public void RevokeRefreshToken()
    {
        HashedRefreshToken = null;
        RefreshTokenExpiresAt = null;
    }
}
