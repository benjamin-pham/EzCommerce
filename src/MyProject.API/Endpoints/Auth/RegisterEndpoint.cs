using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.Features.Auth.Register;

namespace MyProject.API.Endpoints.Auth;

internal sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (
            RegisterUserCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/auth/me", result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: StatusCodes.Status409Conflict,
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.10");
        })
        .WithName("Register")
        .WithTags("Auth")
        .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
    }
}
