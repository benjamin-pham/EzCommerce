using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.Features.Auth.RefreshToken;
using MyProject.Application.Shared.Dtos;

namespace MyProject.API.Endpoints.Auth;

internal sealed class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/refresh-token", async (
            RefreshTokenCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: StatusCodes.Status401Unauthorized,
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.2");
        })
        .WithName("RefreshToken")
        .WithTags("Auth")
        .Produces<TokenResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
