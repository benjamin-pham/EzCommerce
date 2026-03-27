# Query Template Reference

## Query Record

Create `src/{ProjectName}.Application/Features/{EntityPlural}/{OperationName}/{OperationName}Query.cs`:

### Standard Query
```csharp
using {ProjectName}.Application.Abstractions.Messaging;

namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

public sealed record {OperationName}Query(
    // Parameters for filtering/identification
    // Example: Guid BookingId
) : IQuery<{ResponseType}>;
```

### Cached Query
```csharp
using {ProjectName}.Application.Abstractions.Caching;

namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

public sealed record {OperationName}Query(
    Guid {Entity}Id
) : ICachedQuery<{ResponseType}>
{
    public string CacheKey => $"{entityPlural}-{{{Entity}Id}}";
    public TimeSpan? Expiration => null; // or TimeSpan.FromMinutes(5)
}
```

**When to use ICachedQuery:**
- Data that doesn't change frequently (reference data, configuration)
- Expensive queries (complex joins, aggregations)
- High-traffic endpoints

**When to use plain IQuery:**
- Data that changes often
- Queries with user-specific authorization checks
- Simple, fast queries

---

## Query Handler

Create `src/{ProjectName}.Application/Features/{EntityPlural}/{OperationName}/{OperationName}QueryHandler.cs`:

### GetById Pattern
```csharp
using Dapper;
using {ProjectName}.Application.Abstractions.Data;
using {ProjectName}.Application.Abstractions.Messaging;
using {ProjectName}.Domain.Abstractions;

namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

internal sealed class {OperationName}QueryHandler(
    ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<{OperationName}Query, {ResponseType}>
{
    public async Task<Result<{ResponseType}>> Handle(
        {OperationName}Query request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                id          AS Id,
                name        AS Name,
                description AS Description,
                created_at  AS CreatedAt
            FROM {table_snake_case}
            WHERE id = @{Entity}Id
              AND is_deleted = false
            """;

        var result = await connection.QuerySingleOrDefaultAsync<{ResponseType}>(
            sql,
            new { request.{Entity}Id });

        return result is not null
            ? result
            : Result.Failure<{ResponseType}>(new Error("{Entity}.NotFound", "{Entity} không tồn tại."));
    }
}
```

### GetAll / Search Pattern
```csharp
internal sealed class {OperationName}QueryHandler(
    ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<{OperationName}Query, IReadOnlyList<{ResponseType}>>
{
    public async Task<Result<IReadOnlyList<{ResponseType}>>> Handle(
        {OperationName}Query request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                id          AS Id,
                name        AS Name,
                created_at  AS CreatedAt
            FROM {table_snake_case}
            WHERE is_deleted = false
            """;

        var results = await connection.QueryAsync<{ResponseType}>(sql);

        return results.ToList();
    }
}
```

### Query with Authorization (reads current user from IUserContext)
```csharp
internal sealed class {OperationName}QueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IUserContext userContext)
    : IQueryHandler<{OperationName}Query, {ResponseType}>
{
    public async Task<Result<{ResponseType}>> Handle(
        {OperationName}Query request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT ...
            FROM {table_snake_case}
            WHERE id = @UserId
              AND is_deleted = false
            """;

        var result = await connection.QuerySingleOrDefaultAsync<{ResponseType}>(
            sql,
            new { UserId = userContext.UserId });

        return result is not null
            ? result
            : Result.Failure<{ResponseType}>(new Error("{Entity}.NotFound", "{Entity} không tồn tại."));
    }
}
```

### Multi-mapping Pattern (joins)
When the response contains nested objects, use Dapper's multi-mapping:

```csharp
IEnumerable<{ResponseType}> results = await connection
    .QueryAsync<{ResponseType}, {NestedResponse}, {ResponseType}>(
        sql,
        (parent, child) =>
        {
            parent.{ChildProperty} = child;
            return parent;
        },
        new { /* parameters */ },
        splitOn: "{FirstColumnOfChildObject}");
```

---

## Response DTO

Create `src/{ProjectName}.Application/Features/{EntityPlural}/{OperationName}/{ResponseType}.cs` (same directory as the handler):

```csharp
namespace {ProjectName}.Application.Features.{EntityPlural}.{OperationName};

public sealed record {ResponseType}(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    DateTime CreatedAt);
```

**Guidelines:**
- `sealed record` with positional parameters — constructor parameter names must match SQL aliases exactly (Dapper maps by name)
- Always its own separate file — never inline inside the query file
- For nested objects, create separate response records (e.g., `AddressResponse`)
- If a DTO is shared across multiple features (e.g., `TokenResponse` used by Login and RefreshToken), place it in `Shared/Dtos/{ResponseType}.cs` instead

---

## SQL Conventions for Queries

All SQL in query handlers must follow these rules:

- Table names: **snake_case plural** (e.g., `bookings`, `order_items`)
- Column names in DB: **snake_case** (e.g., `created_at`, `user_id`)
- Use SQL **aliases** to map to PascalCase DTO properties: `created_at AS CreatedAt`
- Use **raw string literals** (`"""..."""`) for SQL
- Use **parameterized queries** — never concatenate user input into SQL
- For enums stored as int, cast in SQL or let Dapper handle it
- Prefer `QueryFirstOrDefaultAsync` for single results, `QueryAsync` for lists
