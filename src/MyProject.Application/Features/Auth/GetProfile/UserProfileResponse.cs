using System;

namespace MyProject.Application.Features.Auth.GetProfile;

public sealed record UserProfileResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Username,
    string? Email,
    string? Phone,
    DateOnly? Birthday,
    DateTime CreatedAt);
