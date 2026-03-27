---
name: dotnet-clean-architect
description: >
  Architecture guide and code reviewer for ASP.NET Core Clean Architecture projects (C# / .NET).
  Explains the 4-layer structure (Domain / Application / Infrastructure / API) with Rich Domain
  Model — business logic lives in entities, not anemic service classes. Reviews existing code for
  layer violations and suggests concrete refactoring steps. Use this skill for general layer or
  structure questions: which layer something belongs in, how to organize folders, how to refactor
  toward Clean Architecture, or whether existing code violates dependency rules. Trigger on:
  "code này đặt ở layer nào", "tổ chức folder như thế nào", "review architecture", "layer nào
  nên chứa X", "làm sao tách layer", "clean architecture là gì", "cấu trúc project thế nào",
  "có vi phạm dependency rule không", "tách layer", "refactor theo clean architecture" — or when
  the user pastes code and asks about layer placement or architecture violations.
  Do NOT trigger when the user names a specific feature and asks why it's broken or how it works
  — use dotnet-find-handler instead, which traces the actual handler code.
  Also invoke during /speckit.plan to validate layer decisions and dependency rules,
  during /speckit.tasks to verify task layer assignments match the correct layer,
  and during /speckit.analyze to flag architecture or dependency-rule violations.
metadata:
  related-skills:
    - dotnet-clean-scaffold
    - dotnet-clean-entity
    - dotnet-clean-repository
    - dotnet-clean-feature
    - dotnet-clean-endpoint
    - dotnet-clean-unit-test
    - dotnet-clean-integration-test
    - dotnet-clean-logging
    - dotnet-find-handler
---

# .NET Clean Architecture — Guide & Code Reviewer

Helps you understand, organize, and review Clean Architecture for ASP.NET Core solutions.
Covers the 4 layers, Rich Domain Model principles, and common violation patterns.

---

## The 4 layers at a glance

```
Domain          ← Core business logic. No dependencies on other layers.
Application     ← Use cases / orchestration. Depends on Domain only.
Infrastructure  ← Data access, external services. Depends on Application + Domain.
API             ← HTTP interface. Depends on Application (+ Infrastructure for DI wiring).
```

Dependencies flow **inward only**. Domain knows nothing about the other layers — it's the
most stable, most important layer and should never be "pulled" outward by frameworks or tools.

---

## Layer responsibilities

### Domain layer

The heart of the system. Contains:
- **Entities** — classes with identity and behavior (Rich Domain Model)
- **Enums** — domain-specific enumerations
- **Abstractions** — `IRepository<T>`, `IUnitOfWork`, `Result<T>`, `Error` (generic/shared contracts)
- **Repositories** — entity-specific repository interfaces (`IOrderRepository`, `IProductRepository`)

**Rich Domain Model**: business rules and invariants live *inside* entities as methods,
not in external service classes.

```csharp
// ✅ Rich domain model — entity enforces its own invariants
public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    public void AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                $"Cannot add items to an order in '{Status}' status.");

        _items.Add(new OrderItem(product, quantity));
    }

    public void Submit()
    {
        if (!_items.Any())
            throw new InvalidOperationException("Cannot submit an empty order.");

        Status = OrderStatus.Submitted;
    }
}

// ❌ Anemic model — entity is a data bag, logic lives in a service (anti-pattern)
public class Order : BaseEntity
{
    public List<OrderItem> Items { get; set; } = [];
    public OrderStatus Status { get; set; }
}
public class OrderService  // ← logic scattered here instead
{
    public void Submit(Order order) { order.Status = OrderStatus.Submitted; }
}
```

**Does NOT contain**: EF Core, HttpClient, MediatR, DTOs for API response, connection strings.

---

### Application layer

Orchestrates use cases. Contains:
- **Commands & Queries** — CQRS request objects (`CreateOrderCommand`, `GetOrderQuery`), plus the `ICommand` and `IQuery` marker interfaces they implement
- **Handlers** — `ICommandHandler<TCommand>`, `IQueryHandler<TQuery, TResponse>`
- **DTOs / Responses** — `OrderResponse`, `OrderSummary`
- **Validators** — FluentValidation classes
- **Abstractions** — `ISqlConnectionFactory`, `IEmailService` (interface only, implemented in Infrastructure)
- **Behaviors** — MediatR pipeline behaviors (validation, logging)

Application knows the *what* (the use case), not the *how* (the implementation). It calls
repository interfaces from Domain — never EF Core's `DbContext` directly.

```csharp
// ✅ Handler orchestrates without touching infrastructure details
internal sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var order = Order.Create(command.CustomerId);  // ✅ static factory method

        order.AddItem(command.ProductId, command.Quantity);  // throws if order not in Draft status

        await _orderRepository.AddAsync(order, ct);   // ✅ async, matches IRepository<T>
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
```

**Does NOT contain**: EF Core `DbContext`, `HttpClient`, file I/O, connection strings,
anything that knows *how* to persist or communicate.

---

### Infrastructure layer

Implements interfaces defined in Domain/Application. Contains:
- **AppDbContext** — EF Core context
- **Entity configurations** — `IEntityTypeConfiguration<T>` per entity
- **Repositories** — concrete `IRepository<T>` implementations
- **External service clients** — email, payment gateway, blob storage, etc.
- **Migrations**

This is the only layer allowed to reference EF Core, `HttpClient`, cloud SDKs, or
anything that talks to the outside world.

---

### API layer

HTTP interface only. Contains:
- **Endpoints** — Minimal API classes implementing `IEndpoint`
- **Middleware** — exception handling, correlation ID, etc.
- **DI registration** — `Program.cs` wires up all layers
- **Extension methods** — `AddApplication()`, `AddInfrastructure()`

Endpoints must be thin. They receive HTTP input, send one MediatR command/query, and
return the result. No business logic here — if you're writing `if/else` inside an endpoint,
it probably belongs in the handler or the entity.

```csharp
// ✅ Thin endpoint
public class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (CreateOrderCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Created() : result.ToProblemDetails();
        }).WithTags("Orders");
    }
}
```

---

## Common violations and fixes

| Violation | Where found | Fix |
|-----------|-------------|-----|
| `DbContext` injected directly into a handler | Application | Inject `IRepository<T>` or `IUnitOfWork` instead |
| Business logic (`if/else`, calculations) inside an endpoint | API | Move to entity method or handler |
| `HttpClient` / external SDK in Domain entity | Domain | Define interface in Application, implement in Infrastructure |
| Entity with all public `{ get; set; }` and no methods | Domain | Add behavior methods that enforce business rules — the entity controls its own state |
| Service class that sets entity properties from outside | Application | Push logic down into the entity itself |
| DTO from API request passed directly into Domain | Domain | Map to a Command in Application layer |
| `IRepository` implemented inside Application project | Application | Move implementation to Infrastructure |

---

## How to review existing code

When the user shares code or folder structure, work through this checklist:

1. **Identify the layer** — which project/folder does this live in?
2. **Check the dependency direction** — does it import something from an "outer" layer?
   (e.g., Domain importing EF Core, Application importing `HttpClient`)
3. **Check responsibilities** — is this class doing something that belongs elsewhere?
4. **Check domain richness** — are entities data bags with public setters?
   Is business logic living in service classes instead of entity methods?
5. **Suggest specific moves** — name the exact class, the target layer, and why.

Always explain *why* a change improves the design, not just *what* to move.

---

## Folder structure reference

```
src/
├── {ProjectName}.Domain/
│   ├── Abstractions/          ← IRepository<T>, IUnitOfWork, IUserContext, IDateTimeProvider, Result<T>, Error
│   ├── Entities/              ← Order, Customer, Product (with behavior methods)
│   ├── Enums/                 ← OrderStatus, PaymentMethod
│   └── Repositories/          ← IOrderRepository, IProductRepository (entity-specific interfaces)
│
├── {ProjectName}.Application/
│   ├── Abstractions/
│   │   ├── Data/              ← ISqlConnectionFactory
│   │   ├── Messaging/         ← ICommand, IQuery, ICommandHandler, IQueryHandler
│   │   └── {Feature}/         ← feature-specific interfaces (e.g., Authentication/IJwtTokenService)
│   ├── Behaviors/             ← ValidationBehavior
│   ├── Exceptions/            ← ValidationException, ValidationError
│   ├── Shared/            ← Common
│   │   └── Dtos/    ← Common Dtos
│   │   └── RuleValidator/    ← Common Rule validator
│   └── Features/
│       └── Orders/                ← Feature folder per aggregate
│           ├── Shared/            ← Reusable validators shared across operations in this group
│           └── CreateOrder/
│               ├── CreateOrderCommand.cs
│               ├── CreateOrderCommandHandler.cs
│               ├── CreateOrderCommandValidator.cs
│               ├── OrderResponse.cs
│               └── README.md  ← Business documentation
│
├── {ProjectName}.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/    ← OrderConfiguration : IEntityTypeConfiguration<Order>
│   └── Repositories/          ← OrderRepository : IOrderRepository
│
└── {ProjectName}.API/
    ├── Endpoints/             ← CreateOrderEndpoint, GetOrderEndpoint, IEndpoint, EndpointExtensions
    └── Extensions/            ← GlobalExceptionHandler, CorrelationIdMiddleware, SerilogExtensions
```

---