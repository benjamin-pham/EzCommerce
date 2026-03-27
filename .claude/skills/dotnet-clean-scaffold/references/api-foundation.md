# API Layer — Foundation

Create all files below. Replace `{ProjectName}` with the actual project name.

---

## src/{ProjectName}.API/Endpoints/IEndpoint.cs

```csharp
namespace {ProjectName}.API.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
```

---

## src/{ProjectName}.API/Endpoints/EndpointExtensions.cs

Scans the assembly for all `IEndpoint` implementations and registers them automatically.
This means `Program.cs` never needs to reference individual endpoint classes.

```csharp
using System.Reflection;

namespace {ProjectName}.API.Endpoints;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        Assembly assembly)
    {
        var endpointTypes = assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && t.IsAssignableTo(typeof(IEndpoint)));

        foreach (var type in endpointTypes)
            services.AddTransient(typeof(IEndpoint), type);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (var endpoint in endpoints)
            endpoint.MapEndpoint(builder);

        return app;
    }
}
```

---

## src/{ProjectName}.API/Extensions/GlobalExceptionHandler.cs

Implements `IExceptionHandler` (ASP.NET Core 8+) to catch unhandled exceptions and return
structured problem responses. Registered via `AddExceptionHandler<T>()` + `UseExceptionHandler()`.

```csharp
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using {ProjectName}.Application.Exceptions;

namespace {ProjectName}.API.Extensions;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        ExceptionDetails exceptionDetails = GetExceptionDetails(exception);

        var problemDetails = new ProblemDetails
        {
            Status = exceptionDetails.Status,
            Title = exceptionDetails.Title,
            Detail = exceptionDetails.Detail,
            Type = exceptionDetails.Type
        };

        if (exceptionDetails.Errors is not null)
            problemDetails.Extensions["errors"] = exceptionDetails.Errors;

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ExceptionDetails GetExceptionDetails(Exception exception) =>
        exception switch
        {
            ValidationException validationException => new ExceptionDetails(
                Status: StatusCodes.Status400BadRequest,
                Type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title: "Validation Error",
                Detail: "One or more validation errors has occurred",
                Errors: validationException.Errors),
            _ => new ExceptionDetails(
                Status: StatusCodes.Status500InternalServerError,
                Type: "ServerError",
                Title: "Server error",
                Detail: "An unexpected error has occurred",
                Errors: null)
        };

    internal record ExceptionDetails(
        int Status,
        string Type,
        string Title,
        string Detail,
        IEnumerable<object>? Errors);
}
```

---

## src/{ProjectName}.API/Extensions/CorrelationIdMiddleware.cs

Reads or generates a `X-Correlation-Id` header per request, stores it in `HttpContext.Items`,
echoes it back in the response header, and pushes it into the Serilog log context.

```csharp
using Serilog.Context;

namespace {ProjectName}.API.Extensions;

internal sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();
}
```

---

## src/{ProjectName}.API/Extensions/SerilogExtensions.cs

Extension methods that wrap Serilog host setup and request-logging middleware.
Enriches request logs with the `CorrelationId` stored in `HttpContext.Items`.

```csharp
using Serilog;

namespace {ProjectName}.API.Extensions;

public static class SerilogExtensions
{
    public static IHostBuilder AddSerilogLogging(this IHostBuilder host)
    {
        host.UseSerilog((context, loggerConfig) =>
            loggerConfig.ReadFrom.Configuration(context.Configuration));

        return host;
    }

    public static IApplicationBuilder UseSerilogLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms | CorrelationId: {CorrelationId}";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId) && correlationId is not null)
                    diagnosticContext.Set("CorrelationId", correlationId);
            };
        });

        return app;
    }
}
```

---

## src/{ProjectName}.API/Extensions/OpenApiExtensions.cs

Registers OpenAPI with a JWT bearer security scheme and maps Scalar UI.

```csharp
using Scalar.AspNetCore;

namespace {ProjectName}.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
    {
        services.AddOpenApi();
        return services;
    }

    public static IApplicationBuilder MapOpenApiEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        return app;
    }
}
```

---

## src/{ProjectName}.API/Program.cs

```csharp
using System.Reflection;
using {ProjectName}.API.Endpoints;
using {ProjectName}.API.Extensions;
using {ProjectName}.Application;
using {ProjectName}.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Host.AddSerilogLogging();

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddOpenApiServices();
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();

// ── Pipeline ─────────────────────────────────────────────────────────────────
var app = builder.Build();

app.MapOpenApiEndpoints();
app.UseExceptionHandler(o => { });
app.UseCorrelationId();
app.UseSerilogLogging();
app.UseHttpsRedirection();
app.MapEndpoints();
app.MapHealthChecks("/health");
app.MapFallback(async context =>
{
    context.Response.StatusCode = 404;
    await context.Response.WriteAsync("Resource Not Found");
});
app.Run();

// Expose Program for integration tests
public partial class Program { }
```

---

## src/{ProjectName}.API/appsettings.json

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName"]
  },
  "AllowedHosts": "*"
}
```

---

## src/{ProjectName}.API/appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Database={projectname_lowercase};Username=postgres;Password=your_password"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    }
  }
}
```

Replace `{projectname_lowercase}` with the actual database name in lowercase.
Never commit real credentials — add `appsettings.Development.json` to `.gitignore`.

---

## .gitignore (solution root)

Create a `.gitignore` at the solution root:

```bash
dotnet new gitignore
```

Then add these lines to it:

```
appsettings.Development.json
appsettings.*.json
!appsettings.json
logs/
```
