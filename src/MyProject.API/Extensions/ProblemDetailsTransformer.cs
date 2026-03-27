using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MyProject.API.Extensions;

public sealed class ProblemDetailsTransformer : IOpenApiOperationTransformer
{
    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        operation.Responses ??= new OpenApiResponses();

        operation.Responses.TryAdd("400", await CreateProblemResponse("Bad Request", context));
        operation.Responses.TryAdd("500", await CreateProblemResponse("Internal Server Error", context));
    }

    private static async Task<OpenApiResponse> CreateProblemResponse(
        string description,
        OpenApiOperationTransformerContext context)
    {
        var schema = await context.GetOrCreateSchemaAsync(typeof(ProblemDetails));

        return new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new OpenApiMediaType
                {
                    Schema = schema
                }
            }
        };
    }
}
