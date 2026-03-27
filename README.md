# Commands

```bash
# Build
dotnet build

# Run API (target .NET 10)
dotnet run --project src/MyProject.API/MyProject.API.csproj

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/MyProject.Application.UnitTests/

# Run a specific test
dotnet test --filter "FullyQualifiedName~SomeTestName"
```

No solution file — this is a modern .NET 10 project using directory-level build props.

# Architecture

Clean Architecture with 4 layers (strict unidirectional dependency: API → Application → Domain; Infrastructure → Domain):

- **Domain** — `BaseEntity<TKey>`, `Result<T>`/`Error` for error handling, `IRepository<T>`, `IUnitOfWork`, `ICacheService`, `IDateTimeProvider`, `IUserContext` abstractions. No external dependencies.
- **Application** — MediatR 12 handlers in `Features/` (vertical slices). `ValidationBehavior` pipeline runs FluentValidation before every handler. Throws `ValidationException` on failure.
- **Infrastructure** — EF Core 10 + PostgreSQL via Npgsql. Dapper for read-side queries. `AppDbContext` implements `IUnitOfWork` and auto-sets audit fields (CreatedAt/By, UpdatedAt/By). Generic `Repository<T>` base class.
- **API** — Minimal API endpoints (not controllers). Each endpoint implements `IEndpoint` and is auto-discovered via reflection. Global exception handler converts `ValidationException` → RFC 7807 Problem Details.

# Key Patterns

**Endpoint registration** — Implement `IEndpoint`, place in `src/MyProject.API/Endpoints/`. The endpoint is picked up automatically; no manual registration needed.

**Commands/Queries** — Add a MediatR `IRequest<Result<T>>` + handler in `src/MyProject.Application/Features/{Feature}/`. Add a FluentValidation `AbstractValidator<TRequest>` in the same folder; the pipeline runs it automatically.

**Result pattern** — Domain errors use `Result<T>` (not exceptions). Use `Result.Success(value)` / `Result.Failure(error)` and check `result.IsFailure` in handlers or endpoints.

**Audit trail** — All entities extending `BaseEntity` automatically get `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` set by `AppDbContext.SaveChangesAsync`. `IsDeleted` enables soft deletes.

**EF Core config** — Entity configurations go in `src/MyProject.Infrastructure/Data/Configurations/` using Fluent API with snake_case naming convention.

# Authentication

Three schemes configured: JWT Bearer (primary), API Key (`X-Api-Key` header), Basic Auth (Scalar UI only). Config lives in `appsettings.json` under `Authentication.Jwt`, `Authentication.ApiKeys`, and `Authentication.Scalar` (array of username/password objects).

# API Docs

Scalar UI (not Swagger UI) at `/scalar`. Requires Basic Auth credentials from one of the configured entries in `Authentication.Scalar` in appsettings.

# Database

PostgreSQL. Connection string: `ConnectionStrings:Database` in appsettings. Migrations are EF Core standard (`dotnet ef migrations add`, `dotnet ef database update`).

# Testing

| Project | Scope | Key Dependencies |
|---|---|---|
| `Domain.UnitTests` | Entity logic | xunit, FluentAssertions |
| `Application.UnitTests` | Handlers, validators, behaviors | + NSubstitute |
| `Infrastructure.IntegrationTests` | Repositories, EF Core config | + Testcontainers (PostgreSQL), Respawn |
| `API.IntegrationTests` | End-to-end HTTP | + WebApplicationFactory, Testcontainers, Respawn |
| `ArchitectureTests` | Layer dependency enforcement | NetArchTest.Rules |

Integration tests spin up a real PostgreSQL container via Testcontainers. Respawn resets data between tests.

# Package Management

All NuGet versions are centrally managed in `Directory.Packages.props`. Do not set `Version` on `<PackageReference>` in individual project files; use `VersionOverride` only when necessary.

