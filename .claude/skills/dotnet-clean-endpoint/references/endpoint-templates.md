# Endpoint Templates

All examples use `Product` as the entity. Replace with the actual entity name and adjust properties accordingly.

Commands and Queries come from the Application layer. The endpoint sends them via `ISender` and returns the handler's response DTO — never re-defining request/response DTOs inside the endpoint.

## Folder Structure

```
src/{ProjectName}.API/Endpoints/
├── IEndpoint.cs
├── EndpointExtensions.cs
└── Products/
    ├── GetAllProductsEndpoint.cs
    ├── GetProductByIdEndpoint.cs
    ├── CreateProductEndpoint.cs
    ├── UpdateProductEndpoint.cs
    └── DeleteProductEndpoint.cs
```

---

## GetAll — List All Entities

Query string parameters bind automatically to Query record properties via `[AsParameters]`.

`src/{ProjectName}.API/Endpoints/Products/GetAllProductsEndpoint.cs`

```csharp
using MediatR;
using {ProjectName}.Application.Features.Products.GetAllProducts;

namespace {ProjectName}.API.Endpoints.Products;

public class GetAllProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async ([AsParameters] GetAllProductsQuery query, ISender sender) =>
        {
            var result = await sender.Send(query);
            return Results.Ok(result.Value);
        })
        .WithName("GetAllProducts")
        .WithTags("Products")
        .Produces<IEnumerable<ProductResponse>>();
    }
}
```

---

## GetById — Get Single Entity

Route `{id}` is the only input — no body, no separate request DTO. Construct the query inline.

`src/{ProjectName}.API/Endpoints/Products/GetProductByIdEndpoint.cs`

```csharp
using MediatR;
using {ProjectName}.Application.Features.Products.GetProductById;

namespace {ProjectName}.API.Endpoints.Products;

public class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(id));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status404NotFound);
        })
        .WithName("GetProductById")
        .WithTags("Products")
        .Produces<ProductResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
```

---

## Create — Create New Entity

The Command record is bound directly from the JSON request body. No separate request DTO is needed. The response type is the DTO returned by the handler (`result.Value`).

`src/{ProjectName}.API/Endpoints/Products/CreateProductEndpoint.cs`

```csharp
using MediatR;
using {ProjectName}.Application.Features.Products.CreateProduct;

namespace {ProjectName}.API.Endpoints.Products;

public class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products", async (CreateProductCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/products/{result.Value.Id}", result.Value)
                : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
        })
        .WithName("CreateProduct")
        .WithTags("Products")
        .Produces<CreateProductResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
```

---

## Update — Full Update Entity

The Command is bound directly from the JSON body — including the `Id` property. No `{id:guid}` route segment is needed.

`src/{ProjectName}.API/Endpoints/Products/UpdateProductEndpoint.cs`

```csharp
using MediatR;
using {ProjectName}.Application.Features.Products.UpdateProduct;

namespace {ProjectName}.API.Endpoints.Products;

public class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/products", async (UpdateProductCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error.Code.EndsWith("NotFound")
                    ? Results.Problem(result.Error.Description, statusCode: StatusCodes.Status404NotFound)
                    : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
        })
        .WithName("UpdateProduct")
        .WithTags("Products")
        .Produces<UpdateProductResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
```

> **Note:** If `UpdateProductCommand` returns `Result` (void, no DTO), use `.Produces(StatusCodes.Status204NoContent)` and return `Results.NoContent()` on success instead.

---

## Delete — Soft Delete Entity

Only `id` is needed — construct the Command inline. No body binding.

`src/{ProjectName}.API/Endpoints/Products/DeleteProductEndpoint.cs`

```csharp
using MediatR;
using {ProjectName}.Application.Features.Products.DeleteProduct;

namespace {ProjectName}.API.Endpoints.Products;

public class DeleteProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/products/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status404NotFound);
        })
        .WithName("DeleteProduct")
        .WithTags("Products")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
```

---

## Route Naming Convention

| Entity | Route | Tag |
|--------|-------|-----|
| `Product` | `/api/products` | Products |
| `Category` | `/api/categories` | Categories |
| `OrderItem` | `/api/order-items` | Order Items |
| `UserProfile` | `/api/user-profiles` | User Profiles |

Rules:
- Plural form of the entity name
- Lowercase, kebab-case for multi-word names
- Tag uses title case with spaces

---

## Commands that return void (`Result`, no DTO)

When the Command handler returns `Result` (not `Result<T>`), there is no `result.Value`. Adjust the success branch accordingly:

```csharp
// For Update or Delete that returns Result (void)
var result = await sender.Send(command);
return result.IsSuccess
    ? Results.NoContent()
    : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
```

Use `.Produces(StatusCodes.Status204NoContent)` and `.Produces<ProblemDetails>(StatusCodes.Status400BadRequest)` in the OpenAPI chain instead of `.Produces<T>()`.

---

## Applying to Real Commands

Before writing an endpoint, read the Command/Query file to confirm:
1. The exact class name (e.g., `CreateProductCommand`)
2. The return type (e.g., `ICommand<CreateProductResponse>` or `ICommand`)
3. Whether it has an `Id` property (needed for the `with { Id = id }` pattern on Update)
4. The Response DTO class name (use this in `.Produces<T>()`)

This avoids duplicating property definitions and ensures the endpoint stays in sync with the Application layer automatically.
