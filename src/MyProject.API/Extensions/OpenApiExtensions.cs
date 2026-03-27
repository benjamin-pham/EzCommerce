using Scalar.AspNetCore;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;

namespace MyProject.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {

            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new();

                document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["ApiKey"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.ApiKey,
                        In = ParameterLocation.Header,
                        Name = "X-Api-Key"
                    },
                    ["BearerAuth"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    }
                };

                return Task.CompletedTask;
            });

            options.AddOperationTransformer<ProblemDetailsTransformer>();
        });

        return services;
    }

    public static IApplicationBuilder MapOpenApiEndpoints(this WebApplication app)
    {
        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            options.WithTitle("MyProject API")
                .EnableDarkMode()
                .ShowOperationId()
                .ExpandAllTags()
                .SortTagsAlphabetically()
                .SortOperationsByMethod()
                .WithTheme(ScalarTheme.Moon)
                .PreserveSchemaPropertyOrder()
                .AddPreferredSecuritySchemes("BearerAuth", "ApiKey")
                .AddHttpAuthentication("BearerAuth", auth =>
                {
                    auth.Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
                })
                .AddApiKeyAuthentication("ApiKey", apiKey =>
                {
                    apiKey.Value = "sk-dev-api-key-12345";
                });
        }).RequireAuthorization("ScalarBasic");

        return app;
    }
}
