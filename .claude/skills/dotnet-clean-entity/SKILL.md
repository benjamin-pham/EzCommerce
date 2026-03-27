---
name: dotnet-clean-entity
description: >
  Generate rich domain entities for ASP.NET Core Clean Architecture projects.
  Produces entities with a static factory method (Create) following DDD Rich
  Domain Model patterns. Domain layer only — no EF config, repositories, or DI.
  Use whenever the user wants to create/add a new entity, model, aggregate, or
  domain object in a .NET project. Trigger on: "tạo entity", "thêm entity",
  "add entity", "create entity Product", "tạo model Order", "tạo aggregate",
  "thêm domain object", or any request about domain entities with factory
  methods or encapsulation. Even if user just says "tạo entity X", use this
  skill — all entities follow rich domain model by default.
  Also invoke during /speckit.plan when authoring data-model.md,
  and during /speckit.tasks or /speckit.implement for tasks under
  src/{Project}.Domain/Entities/ or mentioning entities, aggregates, or value objects.
metadata:
  related-skills:
    - dotnet-clean-scaffold
    - dotnet-clean-architect
    - dotnet-clean-repository
    - dotnet-clean-feature
    - dotnet-clean-endpoint
    - dotnet-clean-unit-test
    - dotnet-clean-integration-test
    - dotnet-find-handler
---

# ASP.NET Core — Rich Domain Entity Generator

Generates domain entities following **Rich Domain Model** / DDD patterns:
public setters and a static factory method `Create(...)`.
This skill focuses exclusively on the **Domain layer** — no infrastructure
code is generated.

## Core Principles

1. **Static factory method `Create(...)`** — the only way to instantiate.
   Accepts all required parameters, validates them, and returns a fully
   initialized entity.
2. **No half-constructed objects** — every entity starts in a valid state
   because all required fields are validated and set inside `Create()`.

3. **Guard clauses in `Create()` and behavior methods** — use
   `ArgumentException.ThrowIfNullOrWhiteSpace()` for required strings.

---

## Prerequisite: BaseEntity

Before generating any concrete entity, check that the base entity classes
exist in `src/{ProjectName}.Domain/Abstractions/`. If they don't, create both
classes below in the same file.

### BaseEntity\<TKey\> — generic version

Create `src/{ProjectName}.Domain/Abstractions/BaseEntity.cs`:

```csharp
namespace {ProjectName}.Domain.Abstractions;

public abstract class BaseEntity<TKey> where TKey : notnull
{
    public TKey Id { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
```

### BaseEntity — Guid shorthand

Most entities use `Guid` as key. Provide a non-generic shorthand in the
**same file** (`BaseEntity.cs`), right below the generic class:

```csharp
public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}
```

### Design notes

- `BaseEntity<TKey>` supports any key type (`Guid`, `int`, `long`, `string`).
- `BaseEntity` (no type param) defaults to `Guid` and auto-generates the Id.
- `CreatedAt` is set in the constructor. `CreatedBy`, `UpdatedAt`, `UpdatedBy`
  have public setters — they are populated by infrastructure (e.g., a
  SaveChanges interceptor or EF `SaveChangesAsync` override that reads the
  current user from `IHttpContextAccessor`). The domain layer doesn't know
  about the current user.
- `UpdatedAt` is `DateTime?` (nullable) because a newly created entity has
  no update yet.
- `SoftDelete()` is the only behavior method on BaseEntity.
- Concrete entities inherit `BaseEntity` (Guid) by default. Use
  `BaseEntity<int>` or `BaseEntity<long>` when the user explicitly needs
  a non-Guid key.

---

## Workflow: Adding a New Entity

When the user asks to create an entity (e.g., "tạo entity Product"):

### Step 1 — Detect project structure

Find the `.slnx` (or `.sln`), identify `{ProjectName}`.

### Step 2 — Ensure base classes exist

Check for `BaseEntity.cs` (which should contain both `BaseEntity<TKey>` and
`BaseEntity`). Create if missing (see Prerequisite section above).

### Step 3 — Gather entity requirements

Ask the user (or infer from context) what properties the entity needs:
- What properties does this entity have?
- Which are required vs optional?
- What types? (string, decimal, int, Guid, enum, etc.)
- Are there relationships to other entities?

If the user doesn't specify, infer sensible properties from the entity name
(e.g., `Product` → Name, Description, Price).

### Step 4 — Generate entity class

Create `src/{ProjectName}.Domain/Entities/{EntityName}.cs` following this
template:

```csharp
using {ProjectName}.Domain.Abstractions;

namespace {ProjectName}.Domain.Entities;

public class {EntityName} : BaseEntity
{
    // ── Properties ────────────────────────────────────────
    // Required non-nullable props use = default!; initializer (EF needs it).
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }

    // Navigation properties (no setter — EF loads them)
    public Category? Category { get; }

    // ── Constructor ─────────────────────────────
    public {EntityName}() { }

    // ── Factory Method ────────────────────────────────────
    public static {EntityName} Create(
        string name,
        decimal price,
        Guid categoryId,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new {EntityName}
        {
            Name = name,
            Price = price,
            CategoryId = categoryId,
            Description = description
        };
    }
}
```

---

## Patterns to follow in every entity

1. **`Create(...)` factory method** — accepts all required parameters first,
   then optional parameters with defaults. Returns the entity via object
   initializer. Required parameters should NOT have default values.
   Use `ArgumentException.ThrowIfNullOrWhiteSpace()` for required strings.

2. **Property access levels:**
   - All scalar and reference navigation properties: `{ get; set; }` — EF Core requires a setter to populate them
   - Collection navigations: use a **private backing field** (`List<T>`) with `IReadOnlyCollection<T>` as the
     public property. This enforces encapsulation — external code cannot replace the collection; callers must
     use the entity's behavior methods instead:

     ```csharp
     private readonly List<OrderItem> _items = [];
     public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
     ```

     Expose mutation through a dedicated behavior method:
     ```csharp
     public void AddItem(OrderItem item)
     {
         // validate state here if needed, e.g.:
         // if (Status != OrderStatus.Pending) throw new InvalidOperationException(...);
         _items.Add(item);
     }
     ```

3. **Required vs optional parameters in `Create()`:**
   - Required string props → `string name` (no default)
   - Optional string props → `string? description = null`
   - Required FK → `Guid categoryId` (no default)
   - Enums with a default state → set inside `Create()` body, not as parameter

   Example with an enum:
   ```csharp
   public static Order Create(string customerName, decimal totalAmount)
   {
       ArgumentException.ThrowIfNullOrWhiteSpace(customerName);

       return new Order
       {
           CustomerName = customerName,
           TotalAmount = totalAmount,
           Status = OrderStatus.Pending  // default state, not a parameter
       };
   }
   ```

4. **Behavior methods** — for mutations after creation (e.g., `UpdateProfile`,
   `SetRefreshToken`, `Cancel`), add instance methods that validate state. These are the only ways to change an entity
   after it's created.

---

## Full Example: Order with State Transitions

This example shows a complete entity with an enum status and methods that
transition between states. Use this as a reference when generating entities
that have state/lifecycle management.

`src/{ProjectName}.Domain/Enums/OrderStatus.cs`:

```csharp
namespace {ProjectName}.Domain.Enums;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}
```

`src/{ProjectName}.Domain/Entities/Order.cs`:

```csharp
using {ProjectName}.Domain.Abstractions;
using {ProjectName}.Domain.Enums;

namespace {ProjectName}.Domain.Entities;

public class Order : BaseEntity
{
    // ── Properties ────────────────────────────────────────
    public string CustomerName { get; set; } = default!;
    public string ShippingAddress { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string? Note { get; set; }

    // ── Collection Navigation ─────────────────────────────
    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // ── Constructor ─────────────────────────────
    public Order() { }

    // ── Factory Method ────────────────────────────────────
    public static Order Create(
        string customerName,
        string shippingAddress,
        decimal totalAmount,
        string? note = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(shippingAddress);

        return new Order
        {
            CustomerName = customerName,
            ShippingAddress = shippingAddress,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            Note = note
        };
    }

    // ── State Transitions ─────────────────────────────────
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot confirm order in '{Status}' status. Only Pending orders can be confirmed.");

        Status = OrderStatus.Confirmed;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException(
                $"Cannot ship order in '{Status}' status. Only Confirmed orders can be shipped.");

        Status = OrderStatus.Shipped;
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException(
                $"Cannot deliver order in '{Status}' status. Only Shipped orders can be delivered.");

        Status = OrderStatus.Delivered;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new InvalidOperationException(
                $"Cannot cancel order in '{Status}' status.");

        Status = OrderStatus.Cancelled;
    }
}
```

Key patterns in this example:
- `Status` starts as `Pending` inside `Create()` — the caller doesn't choose it.
- Each transition method checks the current state before allowing the change.
- The transition methods throw `InvalidOperationException` with a clear
  message explaining why the transition is not allowed.
- The entity controls its own state machine through domain methods.

---

## Entity Naming

- If the user provides the entity name in Vietnamese, translate to English
  PascalCase and confirm with the user.
- Class name: `PascalCase` singular (e.g., `Product`, `OrderItem`).

---

## Important Reminders

- Use **file-scoped namespaces** everywhere.
- Use **C# 13** features where appropriate (collection expressions `[]`, etc.)
  but NOT primary constructors on entities — entities need a public
  parameterless constructor for EF Core.
- This skill generates **Domain layer only**. Do NOT create EF configurations,
  repositories, DbSet registrations, or DI wiring.
- When the entity has relationships, define navigation properties and FK
  `Guid` properties in the entity, but leave EF relationship configuration
  to the infrastructure layer.
- If the entity has an enum (e.g., `OrderStatus`), define it in a separate
  file at `src/{ProjectName}.Domain/Enums/{EnumName}.cs`.
