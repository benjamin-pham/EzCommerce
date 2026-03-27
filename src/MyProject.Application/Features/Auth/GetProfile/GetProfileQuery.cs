using MyProject.Application.Abstractions.Messaging;

namespace MyProject.Application.Features.Auth.GetProfile;

public sealed record GetProfileQuery : IQuery<UserProfileResponse>;


