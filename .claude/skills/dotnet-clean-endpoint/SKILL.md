---
name: dotnet-clean-endpoint
description: >
  Generate minimal API endpoints for ASP.NET Core Clean Architecture projects
  using the class-per-endpoint pattern. Each endpoint is a class implementing
  IEndpoint, organized in Endpoints/{Entity}/ folders. Covers CRUD generation,
  auto-registration via assembly scanning, request/response DTOs, and route
  conventions. Trigger whenever the user wants to add an endpoint, wire up a new route,
  or register HTTP routes for an entity — including Vietnamese like "tạo endpoint",
  "thêm API cho entity", "thêm route", "đăng ký route", "tạo CRUD endpoint".
  Do NOT trigger for entity generation, or project scaffolding.
  Also invoke during /speckit.plan when designing API contracts or HTTP surface,
  and during /speckit.tasks or /speckit.implement for tasks under
  src/{Project}.Api/Endpoints/ or mentioning HTTP routes or endpoints.
metadata:
  related-skills:
    - dotnet-clean-scaffold
    - dotnet-clean-architect
    - dotnet-clean-entity
    - dotnet-clean-feature
    - dotnet-clean-repository
    - dotnet-clean-logging
    - dotnet-clean-integration-test
---

# Minimal API Endpoints — Class-per-Endpoint Pattern

Generates endpoints using a structured pattern where each HTTP operation lives in its own class. This keeps endpoint logic focused, testable, and easy to find — instead of one giant static class with all routes for an entity.

## Prerequisites

Before generating endpoints, ensure:
1. **Entity exists** in the Domain layer (`dotnet-clean-entity`)
2. **MediatR Commands/Queries exist** for the entity in the Application layer (`dotnet-clean-feature`) —
   endpoints are thin wrappers that call `ISender.Send(command)`, so the commands must already exist
3. **Repository is registered in DI** (`dotnet-clean-repository`) — if the underlying command handler
   injects `I{Entity}Repository`, the DI registration must be in place or the app will fail at startup

The recommended order for a new entity: `dotnet-clean-entity` → `dotnet-clean-repository` →
`dotnet-clean-feature` → `dotnet-clean-endpoint`.

## Step 1 — IEndpoint Infrastructure (one-time setup)

Read `references/endpoint-infrastructure.md` for the complete code. Create these files if they don't exist yet:

1. **`IEndpoint` interface** — `src/{ProjectName}.API/Endpoints/IEndpoint.cs`
2. **`EndpointExtensions`** — `src/{ProjectName}.API/Endpoints/EndpointExtensions.cs`
   - Scans the assembly for all classes implementing `IEndpoint`
   - Creates an instance of each and calls `MapEndpoint(app)`
3. **Update `Program.cs`** — replace manual endpoint mapping with `app.MapEndpoints()`

This is a one-time setup. Once the infrastructure exists, skip to Step 2 for each new entity.

## Step 2 — Generate Endpoint Classes

For each entity, create endpoint classes in `src/{ProjectName}.API/Endpoints/{Entity}/`. Each class handles exactly one HTTP operation.

Read `references/endpoint-templates.md` for the complete CRUD templates. The standard set for an entity:

| Class | Route | HTTP Method | Purpose |
|-------|-------|-------------|---------|
| `GetAll{Entity}Endpoint` | `GET /api/{entities}` | GET | List all (with optional filtering) |
| `Get{Entity}ByIdEndpoint` | `GET /api/{entities}/{id}` | GET | Get single by ID |
| `Create{Entity}Endpoint` | `POST /api/{entities}` | POST | Create new |
| `Update{Entity}Endpoint` | `PUT /api/{entities}/{id}` | PUT | Full update |
| `Delete{Entity}Endpoint` | `DELETE /api/{entities}/{id}` | DELETE | Soft delete |

Key patterns:
- Class name ends with `Endpoint` — this is a convention, not enforced by the interface
- Each class implements `IEndpoint` and has one method: `MapEndpoint`
- Dependencies (repositories, services, loggers) come through **delegate parameters**, not constructor injection — the class itself is only instantiated once during registration
- Use `MapGroup` to set the base route and tags, then map the single operation on that group
- Route names follow REST conventions: plural, kebab-case for multi-word entities (`/api/order-items`)

## Step 3 — Request / Response Convention

**Do not create separate request DTOs in the endpoint.** Instead, bind the Application layer's Command or Query directly. This avoids duplicating property definitions and keeps the endpoint thin.

### Request binding rules

| HTTP method | Binding |
|-------------|---------|
| POST | `({OperationName}Command command, ISender sender)` — ASP.NET binds the JSON body to the Command record |
| PUT / PATCH | `({OperationName}Command command, ISender sender)` — entire body including `Id` comes from the JSON body; no `{id:guid}` in the route |
| DELETE | `(Guid id, ISender sender)` — no body; construct `new Delete{Entity}Command(id)` inline |
| GET (list) | `([AsParameters] Get{Entities}Query query, ISender sender)` — query string params bind to Query record properties via `[AsParameters]` |
| GET (by id) | `(Guid id, ISender sender)` — route param only; construct `new Get{Entity}ByIdQuery(id)` inline. **No Command/Query binding needed.** |

### Response binding rules

The Command/Query handler returns `Result<{ResponseType}>` or `Result`. Use `result.Value` as the response body — the response type is the DTO already defined alongside the handler in the Application layer (e.g., `CreateProductResponse`). Never define a separate response DTO in the endpoint.

```csharp
// ✓ correct — sends Command directly, returns handler's response DTO
app.MapPost("/api/products", async (CreateProductCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.IsSuccess
        ? Results.Created($"/api/products/{result.Value.Id}", result.Value)
        : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
})
.Produces<CreateProductResponse>(StatusCodes.Status201Created)
.Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
.Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

// ✗ wrong — duplicates Command properties into a separate record
public record CreateProductRequest(string Name, decimal Price, Guid CategoryId);
app.MapPost("/api/products", async (CreateProductRequest request, ...) => { ... });
```

## Step 4 — Validate

1. Verify all endpoint files are created in the correct folders
2. Verify `IEndpoint` and `EndpointExtensions` exist
3. Verify `Program.cs` calls `app.MapEndpoints()`
4. Run `dotnet build` to confirm compilation

## Important Reminders

- **One class = one operation.** Don't put multiple MapGet/MapPost in one class. The whole point is separation.
- **Delegate parameters for DI.** The endpoint class is instantiated once at startup for registration. Runtime dependencies (repos, services) go in the lambda parameters inside `MapEndpoint`, where the DI container resolves them per-request.
- **Route conventions**: `/api/{entities}` — plural, lowercase. Multi-word entities use kebab-case (`/api/order-items` for `OrderItem`).
- **OpenAPI / Produces — mandatory on every endpoint**: Every route registration MUST end with `.WithName()`, `.WithTags()`, and **all applicable `.Produces<T>()` / `.Produces(StatusCodes.StatusXXX)` calls** so Scalar/OpenAPI documents every possible response. Never omit these. Use the response-code table below as a checklist:

  | HTTP Method | Always declare |
  |-------------|---------------|
  | GET (list)  | `.Produces<IEnumerable<TResponse>>()` + `.Produces<ProblemDetails>(500)` |
  | GET (by id) | `.Produces<TResponse>()` + `.Produces<ProblemDetails>(404)` + `.Produces<ProblemDetails>(500)` |
  | POST        | `.Produces<TResponse>(201)` + `.Produces<ProblemDetails>(400)` + `.Produces<ProblemDetails>(500)` |
  | PUT / PATCH | `.Produces<TResponse>()` + `.Produces<ProblemDetails>(404)` + `.Produces<ProblemDetails>(400)` + `.Produces<ProblemDetails>(500)` |
  | DELETE      | `.Produces(204)` + `.Produces<ProblemDetails>(404)` + `.Produces<ProblemDetails>(500)` |

  Use `StatusCodes.StatusXXX` constants (not raw integers) in actual code — the table above uses integers only for readability.
  Add extra `.Produces<ProblemDetails>(StatusCodes.StatusXXX)` for any additional error codes the handler can return (e.g. 409 Conflict, 422 Unprocessable Entity).
  The 500 entry documents the global exception-handler middleware; it does not mean the endpoint itself throws — it's an OpenAPI contract signal.

- **Soft delete**: the Delete endpoint sets `IsDeleted = true` via the repository, not a hard delete.
- Target **.NET 10** with **C# 13** features (file-scoped namespaces, primary constructors).
- If user provides entity name in Vietnamese, translate to English PascalCase and confirm with the user.
