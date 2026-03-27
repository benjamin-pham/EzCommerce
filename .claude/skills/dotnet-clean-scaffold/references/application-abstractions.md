# Application Layer — Abstractions & DI

Create all files below. Replace `{ProjectName}` with the actual project name.

---

## src/{ProjectName}.Application/Abstractions/Messaging/ICommand.cs

```csharp
using MediatR;
using {ProjectName}.Domain.Abstractions;

namespace {ProjectName}.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
```

---

## src/{ProjectName}.Application/Abstractions/Messaging/IQuery.cs

```csharp
using MediatR;
using {ProjectName}.Domain.Abstractions;

namespace {ProjectName}.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
```

---

## src/{ProjectName}.Application/Abstractions/Messaging/ICommandHandler.cs

```csharp
using MediatR;
using {ProjectName}.Domain.Abstractions;

namespace {ProjectName}.Application.Abstractions.Messaging;

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
```

---

## src/{ProjectName}.Application/Abstractions/Messaging/IQueryHandler.cs

```csharp
using MediatR;

namespace {ProjectName}.Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
```

---

## src/{ProjectName}.Application/Abstractions/Data/ISqlConnectionFactory.cs

```csharp
using System.Data;

namespace {ProjectName}.Application.Abstractions.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
```

---

## src/{ProjectName}.Application/Exceptions/ValidationError.cs

```csharp
namespace {ProjectName}.Application.Exceptions;

public sealed record ValidationError(string PropertyName, string ErrorMessage);
```

---

## src/{ProjectName}.Application/Exceptions/ValidationException.cs

A custom exception thrown by `ValidationBehavior` — using a project-defined type keeps the
Application layer independent of FluentValidation's own exception hierarchy.

```csharp
namespace {ProjectName}.Application.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(IEnumerable<ValidationError> errors)
    {
        Errors = errors;
    }

    public IEnumerable<ValidationError> Errors { get; }
}
```

---

## src/{ProjectName}.Application/Behaviors/ValidationBehavior.cs

This pipeline behavior runs FluentValidation validators before every MediatR request.
It collects all failures and throws a custom `ValidationException` (not FluentValidation's).
The `GlobalExceptionHandler` in the API layer catches this exception and returns 400.

```csharp
using FluentValidation;
using MediatR;
using {ProjectName}.Application.Exceptions;

namespace {ProjectName}.Application.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        List<ValidationError> validationErrors = validators
            .Select(validator => validator.Validate(context))
            .Where(result => result.Errors.Count != 0)
            .SelectMany(result => result.Errors)
            .Select(failure => new ValidationError(
                ToCamelCase(failure.PropertyName),
                failure.ErrorMessage))
            .ToList();

        if (validationErrors.Count != 0)
            throw new Exceptions.ValidationException(validationErrors);

        return await next();
    }

    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name)
            ? name
            : char.ToLowerInvariant(name[0]) + name[1..];
}
```

---

## src/{ProjectName}.Application/DependencyInjection.cs

```csharp
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using {ProjectName}.Application.Behaviors;

namespace {ProjectName}.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
```
