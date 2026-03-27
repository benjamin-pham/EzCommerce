# Command Template Reference

## Command Record

Create `src/{ProjectName}.Application/Features/{EntityPlural}/{OperationName}/{OperationName}Command.cs`:

```csharp
using {ProjectName}.Application.Abstractions.Messaging;

namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

public sealed record {OperationName}Command(
    // Properties based on user requirements
    // Example for CreateProduct:
    // string Name,
    // decimal Price,
    // string? Description
) : ICommand<{ResponseType}>;
```

**Guidelines:**
- Use `ICommand` (no generic) if the command returns nothing (void → `Result`)
- Use `ICommand<Guid>` for create operations that return the new entity's ID
- Use `ICommand<TResponse>` for operations returning a custom DTO
- Properties are positional parameters in the record — keep them simple value types
- Do NOT put domain objects as command properties — use primitive types or simple DTOs

---

## Command Handler

Create `src/{ProjectName}.Application/Features/{EntityPlural}/{OperationName}/{OperationName}CommandHandler.cs`:

```csharp
using {ProjectName}.Application.Abstractions.Messaging;
using {ProjectName}.Domain.Abstractions;
using {ProjectName}.Domain.Repositories;
using {ProjectName}.Domain.Entities;

namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

internal sealed class {OperationName}CommandHandler(
    I{Entity}Repository {entityCamel}Repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<{OperationName}Command, {ResponseType}>
{
    public async Task<Result<{ResponseType}>> Handle(
        {OperationName}Command request,
        CancellationToken cancellationToken)
    {
        // 1. Load entity/entities from repository
        // 2. Perform domain logic (call domain methods, create new entities)
        // 3. Persist via repository + UnitOfWork
        // 4. Return Result.Success or Result.Failure

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id; // or Result.Success() for void commands
    }
}
```

**Handler patterns by operation type:**

### Create
```csharp
public async Task<Result<{ResponseType}>> Handle(Create{Entity}Command request, CancellationToken cancellationToken)
{
    // Use domain factory method — never use new {Entity}()
    var entity = {Entity}.Create(/* map request properties */);

    await {entityCamel}Repository.AddAsync(entity, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return new {ResponseType}(entity.Id, ...);
}
```

### Update
```csharp
public async Task<Result> Handle(Update{Entity}Command request, CancellationToken cancellationToken)
{
    var entity = await {entityCamel}Repository.GetByIdAsync(request.Id, cancellationToken);

    if (entity is null)
        return Result.Failure(new Error("{Entity}.NotFound", "{Entity} không tồn tại."));

    // Call domain behavior methods — never set properties directly from outside
    entity.UpdateDetails(request.Name, request.Description);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
}
```

### Delete
```csharp
public async Task<Result> Handle(Delete{Entity}Command request, CancellationToken cancellationToken)
{
    var entity = await {entityCamel}Repository.GetByIdAsync(request.Id, cancellationToken);

    if (entity is null)
        return Result.Failure(new Error("{Entity}.NotFound", "{Entity} không tồn tại."));

    entity.SoftDelete();
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
}
```

**Key rules:**
- Always check if entity exists before update/delete — return `Result.Failure` with domain error
- Use domain factory methods (`Entity.Create(...)`) instead of `new Entity()`
- Inject only what's needed — don't inject services "just in case"
- Wrap risky operations in try/catch only when specific exceptions are expected (e.g., `ConcurrencyException`)
- Call `_unitOfWork.SaveChangesAsync()` — NOT the repository's save method

### Getting the Current Logged-In User

When a command needs the current user's ID (e.g., update own profile, place own order), inject **`IUserContext`** into the **handler** — do NOT put `Guid UserId` in the Command record.

```csharp
// ✓ correct — primary constructor, handler reads identity from IUserContext
internal sealed class UpdateProfileCommandHandler(
    IUserRepository userRepository,
    IUserContext userContext,          // ← inject here
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateProfileCommand>
{
    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userContext.UserId, cancellationToken);
        ...
    }
}

// ✓ command only carries business data — no UserId
public sealed record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string? Email) : ICommand;
```

```csharp
// ✗ wrong — passing userId through command leaks identity concern into Application API
public sealed record UpdateProfileCommand(Guid UserId, string FirstName ...) : ICommand;
```

`IUserContext` is defined in `{ProjectName}.Domain.Abstractions` and resolved from `IHttpContextAccessor` by the Infrastructure implementation — the Application layer sees only the interface.

---

## Response DTO (when command returns a DTO)

When a command returns a custom DTO (not `Result` void or `Result<Guid>`), create a separate file in **the same directory as the handler**:

Create `src/{ProjectName}.Application/Features/{EntityPlural}/{OperationName}/{ResponseType}.cs`:

```csharp
namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

public sealed record {ResponseType}(
    Guid Id,
    string Name,
    // ... other fields returned to caller
);
```

**Guidelines:**
- `sealed record` with positional parameters — always its own file, never inline in the command file
- Instantiated manually in the handler (not by Dapper) — property names don't need to match SQL
- If the DTO is shared across multiple features (e.g., `TokenResponse` used by Login and RefreshToken), place it in `Shared/Dtos/{ResponseType}.cs` instead

---

## Command Validator

Create `src/{ProjectName}.Application/Features/{EntityPlural}/{OperationName}/{OperationName}CommandValidator.cs`:

```csharp
using FluentValidation;

namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

internal class {OperationName}CommandValidator : AbstractValidator<{OperationName}Command>
{
    public {OperationName}CommandValidator()
    {
        // Validate required fields
        RuleFor(x => x.PropertyName).NotEmpty();

        // Validate string lengths
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        // Validate email format
        RuleFor(x => x.Email).EmailAddress();

        // Validate numeric ranges
        RuleFor(x => x.Price).GreaterThan(0);

        // Validate date logic
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate);

        // Validate password strength
        RuleFor(x => x.Password).NotEmpty().MinimumLength(5);

        // Validate Guid not empty
        RuleFor(x => x.EntityId).NotEmpty();
    }
}
```

**Guidelines:**
- Validator is `internal class` (not sealed) — matches project convention
- Only validate **shape and format** — not business rules that need DB access
- Business rule validation belongs in the Handler (e.g., "does this entity exist?")
- Every command property that's required should have at least `NotEmpty()`
- String properties should have `MaximumLength()` matching the DB column constraint
- The `ValidationBehavior` pipeline will run validators automatically before the handler


