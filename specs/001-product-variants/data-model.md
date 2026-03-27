# Mô hình dữ liệu: Quản lý sản phẩm có biến thể

**Tính năng**: `001-product-variants` | **Ngày**: 2026-03-27

---

## Sơ đồ quan hệ

```
Category (n) ──────────────── (n) Product
                                    │
                                    │ (1)
                                    │
                             VariantAttribute (n per product)
                                    │
                                    │ (1)
                                    │
                           VariantAttributeValue (n per attribute)
                                    │
                                    │ (n) ──────── (n)
                                    │
                              ProductVariant
```

**Quan hệ**:
- `Product` ↔ `Category`: nhiều-nhiều (join table `product_categories`, không có payload)
- `Product` → `VariantAttribute`: một-nhiều (per-product, không chia sẻ)
- `VariantAttribute` → `VariantAttributeValue`: một-nhiều
- `ProductVariant` ↔ `VariantAttributeValue`: nhiều-nhiều (join table `product_variant_attribute_values`)
- `Product` → `ProductVariant`: một-nhiều

---

## Entity: Category

**Vị trí**: `src/MyProject.Domain/Entities/Category.cs`

```csharp
using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    private readonly List<Product> _products = [];
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    public Category() { }

    public static Category Create(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Category
        {
            Name = name,
            Description = description
        };
    }

    public void Update(string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Description = description;
    }
}
```

**Bảng DB**: `categories`
| Cột | Kiểu | Ràng buộc |
|-----|------|-----------|
| `id` | `uuid` | PK |
| `name` | `varchar(200)` | NOT NULL |
| `description` | `text` | NULL |
| `created_at` | `timestamptz` | NOT NULL |
| `created_by` | `varchar(100)` | NULL |
| `updated_at` | `timestamptz` | NULL |
| `updated_by` | `varchar(100)` | NULL |
| `is_deleted` | `boolean` | NOT NULL DEFAULT false |

---

## Entity: Product

**Vị trí**: `src/MyProject.Domain/Entities/Product.cs`

```csharp
using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    private readonly List<Category> _categories = [];
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    private readonly List<VariantAttribute> _variantAttributes = [];
    public IReadOnlyCollection<VariantAttribute> VariantAttributes => _variantAttributes.AsReadOnly();

    private readonly List<ProductVariant> _variants = [];
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    public Product() { }

    public static Product Create(
        string name,
        string sku,
        string slug,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Product
        {
            Name = name,
            Sku = sku,
            Slug = slug,
            Description = description,
            IsActive = true
        };
    }

    public void Update(string name, string slug, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        Name = name;
        Slug = slug;
        Description = description;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void ToggleStatus() => IsActive = !IsActive;

    public void AddCategory(Category category) => _categories.Add(category);
    public void RemoveCategory(Category category) => _categories.Remove(category);

    public void AddVariantAttribute(VariantAttribute attribute) =>
        _variantAttributes.Add(attribute);

    public void AddVariant(ProductVariant variant) => _variants.Add(variant);

    public void RemoveVariant(ProductVariant variant) => _variants.Remove(variant);
}
```

**Bảng DB**: `products`
| Cột | Kiểu | Ràng buộc |
|-----|------|-----------|
| `id` | `uuid` | PK |
| `name` | `varchar(500)` | NOT NULL |
| `sku` | `varchar(100)` | NOT NULL, UNIQUE |
| `slug` | `varchar(600)` | NOT NULL, UNIQUE |
| `description` | `text` | NULL |
| `is_active` | `boolean` | NOT NULL DEFAULT true |
| `created_at` | `timestamptz` | NOT NULL |
| `created_by` | `varchar(100)` | NULL |
| `updated_at` | `timestamptz` | NULL |
| `updated_by` | `varchar(100)` | NULL |
| `is_deleted` | `boolean` | NOT NULL DEFAULT false |

**Bảng join**: `product_categories`
| Cột | Kiểu | Ràng buộc |
|-----|------|-----------|
| `products_id` | `uuid` | PK (composite), FK → products |
| `categories_id` | `uuid` | PK (composite), FK → categories |

---

## Entity: VariantAttribute

**Vị trí**: `src/MyProject.Domain/Entities/VariantAttribute.cs`

```csharp
using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public class VariantAttribute : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;

    public Product? Product { get; set; }

    private readonly List<VariantAttributeValue> _values = [];
    public IReadOnlyCollection<VariantAttributeValue> Values => _values.AsReadOnly();

    public VariantAttribute() { }

    public static VariantAttribute Create(Guid productId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new VariantAttribute
        {
            ProductId = productId,
            Name = name
        };
    }

    public void AddValue(VariantAttributeValue value) => _values.Add(value);
}
```

**Bảng DB**: `variant_attributes`
| Cột | Kiểu | Ràng buộc |
|-----|------|-----------|
| `id` | `uuid` | PK |
| `product_id` | `uuid` | NOT NULL, FK → products |
| `name` | `varchar(200)` | NOT NULL |
| `created_at` | `timestamptz` | NOT NULL |
| `created_by` | `varchar(100)` | NULL |
| `updated_at` | `timestamptz` | NULL |
| `updated_by` | `varchar(100)` | NULL |
| `is_deleted` | `boolean` | NOT NULL DEFAULT false |

---

## Entity: VariantAttributeValue

**Vị trí**: `src/MyProject.Domain/Entities/VariantAttributeValue.cs`

```csharp
using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public class VariantAttributeValue : BaseEntity
{
    public Guid VariantAttributeId { get; set; }
    public string Value { get; set; } = default!;

    public VariantAttribute? VariantAttribute { get; set; }

    public VariantAttributeValue() { }

    public static VariantAttributeValue Create(Guid variantAttributeId, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new VariantAttributeValue
        {
            VariantAttributeId = variantAttributeId,
            Value = value
        };
    }
}
```

**Bảng DB**: `variant_attribute_values`
| Cột | Kiểu | Ràng buộc |
|-----|------|-----------|
| `id` | `uuid` | PK |
| `variant_attribute_id` | `uuid` | NOT NULL, FK → variant_attributes |
| `value` | `varchar(200)` | NOT NULL |
| `created_at` | `timestamptz` | NOT NULL |
| `created_by` | `varchar(100)` | NULL |
| `updated_at` | `timestamptz` | NULL |
| `updated_by` | `varchar(100)` | NULL |
| `is_deleted` | `boolean` | NOT NULL DEFAULT false |

---

## Entity: ProductVariant

**Vị trí**: `src/MyProject.Domain/Entities/ProductVariant.cs`

```csharp
using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public bool IsActive { get; set; }

    public Product? Product { get; set; }

    private readonly List<VariantAttributeValue> _attributeValues = [];
    public IReadOnlyCollection<VariantAttributeValue> AttributeValues =>
        _attributeValues.AsReadOnly();

    public ProductVariant() { }

    public static ProductVariant Create(
        Guid productId,
        string sku,
        IEnumerable<VariantAttributeValue> attributeValues)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        var variant = new ProductVariant
        {
            ProductId = productId,
            Sku = sku,
            IsActive = true
        };

        foreach (var av in attributeValues)
            variant._attributeValues.Add(av);

        return variant;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
```

**Bảng DB**: `product_variants`
| Cột | Kiểu | Ràng buộc |
|-----|------|-----------|
| `id` | `uuid` | PK |
| `product_id` | `uuid` | NOT NULL, FK → products |
| `sku` | `varchar(100)` | NOT NULL, UNIQUE |
| `is_active` | `boolean` | NOT NULL DEFAULT true |
| `created_at` | `timestamptz` | NOT NULL |
| `created_by` | `varchar(100)` | NULL |
| `updated_at` | `timestamptz` | NULL |
| `updated_by` | `varchar(100)` | NULL |
| `is_deleted` | `boolean` | NOT NULL DEFAULT false |

**Bảng join**: `product_variant_attribute_values`
| Cột | Kiểu | Ràng buộc |
|-----|------|-----------|
| `product_variants_id` | `uuid` | PK (composite), FK → product_variants |
| `attribute_values_id` | `uuid` | PK (composite), FK → variant_attribute_values |

---

## Repository Interfaces

### ICategoryRepository

**Vị trí**: `src/MyProject.Domain/Repositories/ICategoryRepository.cs`

```csharp
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;

namespace MyProject.Domain.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetAllActiveAsync(CancellationToken ct = default);
}
```

### IProductRepository

**Vị trí**: `src/MyProject.Domain/Repositories/IProductRepository.cs`

```csharp
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;

namespace MyProject.Domain.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<bool> IsSkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> IsSlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
}
```

---

## Index PostgreSQL cần tạo (trong EF Migration)

```sql
-- Unique indexes (enforced at DB level as tầng bảo vệ cuối)
CREATE UNIQUE INDEX ix_products_sku ON products(sku) WHERE is_deleted = false;
CREATE UNIQUE INDEX ix_products_slug ON products(slug) WHERE is_deleted = false;
CREATE UNIQUE INDEX ix_product_variants_sku ON product_variants(sku) WHERE is_deleted = false;

-- Performance indexes
CREATE INDEX ix_products_is_deleted ON products(is_deleted);
CREATE INDEX ix_products_is_active ON products(is_active) WHERE is_deleted = false;
CREATE INDEX ix_product_categories_category_id ON product_categories(categories_id);
CREATE INDEX ix_product_variants_product_id ON product_variants(product_id);
CREATE INDEX ix_variant_attributes_product_id ON variant_attributes(product_id);

-- Full-text search (cần pg_trgm extension)
CREATE INDEX ix_products_name_trgm ON products USING gin(name gin_trgm_ops) WHERE is_deleted = false;
```
