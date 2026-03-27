using System;

namespace MyProject.Application.Shared.Dtos;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType);