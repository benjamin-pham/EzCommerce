using System;

namespace MyProject.Application.Features.Auth.Register;

public sealed record RegisterUserResponse(
    Guid UserId,
    string Username,
    string FirstName,
    string LastName);
