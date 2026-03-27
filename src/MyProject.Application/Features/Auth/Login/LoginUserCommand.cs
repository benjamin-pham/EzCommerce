using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Shared.Dtos;

namespace MyProject.Application.Features.Auth.Login;

public sealed record LoginUserCommand(
    string Username,
    string Password) : ICommand<TokenResponse>;


