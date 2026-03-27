using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Auth.Register;

public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Username,
    string Password,
    string? Email,
    string? Phone,
    DateOnly? Birthday) : ICommand<RegisterUserResponse>;


