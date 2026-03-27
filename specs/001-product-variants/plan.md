# Kế hoạch triển khai: Quản lý sản phẩm có biến thể

**Nhánh**: `001-product-variants` | **Ngày**: 2026-03-27 | **Spec**: [spec.md](spec.md)
**Đầu vào**: Đặc tả tính năng từ `/specs/001-product-variants/spec.md`

## Tóm tắt

Xây dựng module quản lý sản phẩm có biến thể cho hệ thống EzCommerce. Cho phép admin tạo, xem,
cập nhật và xóa sản phẩm với các thuộc tính biến thể tự do (per-product), tự động sinh tổ hợp
biến thể (cartesian product), quản lý danh mục phẳng nhiều-nhiều. Toàn bộ dùng
Clean Architecture 4-layer + CQRS (EF Core cho write, Dapper cho read).

## Bối cảnh kỹ thuật

**Ngôn ngữ/Phiên bản**: C# 13 / .NET 10
**Phụ thuộc chính**: MediatR 12.4.1, EF Core 10.x (Npgsql), Dapper, FluentValidation
**Lưu trữ**: PostgreSQL 10.x
**Kiểm thử**: xUnit + NSubstitute + FluentAssertions (unit), xUnit + Testcontainers + Respawn (integration)
**Nền tảng mục tiêu**: Linux server (Docker)
**Loại dự án**: web-service (Minimal API — class-per-endpoint pattern)
**Mục tiêu hiệu năng**: Tìm kiếm < 1 giây với tối đa 10,000 sản phẩm
**Ràng buộc**: SKU duy nhất toàn hệ thống (bao gồm cả biến thể), slug duy nhất toàn hệ thống
**Quy mô**: ~10,000 sản phẩm, chỉ dành cho admin

## Kiểm tra hiến pháp

*GATE: Phải pass trước Phase 0. Kiểm tra lại sau Phase 1 design.*

Xác minh các điều sau (dựa trên MyProject Constitution v1.0.0):

- [x] **I. Clean Architecture** — Domain ← Application ← Infrastructure/API.
      5 entities đặt trong Domain. Repo interface trong Domain. EF Core chỉ trong Infrastructure.
- [x] **II. Rich Domain Model** — `Product.Create(...)`, `Category.Create(...)`, v.v.
      Mutations qua domain methods (`AddVariantAttribute`, `RemoveVariant`, `Deactivate`...).
- [x] **III. .NET 10 Practices** — File-scoped namespaces, records cho DTOs, primary constructors
      cho handlers. Nullable enabled. Methods ≤ 30 lines, classes ≤ 200 lines.
- [x] **IV. Testing Discipline** — Unit tests cho Domain entities + Application handlers.
      Integration tests dùng Testcontainers PostgreSQL thật.
- [x] **V. Observability** — Serilog structured logging trong tất cả handlers.
      Log `{@Command}`, `{ProductId}`, `{Sku}`. Không log PII.
- [x] **Tech Stack** — Không thêm công nghệ mới. Đúng stack PostgreSQL + EF Core + Dapper + MediatR.

## Cấu trúc dự án

### Tài liệu (tính năng này)

```text
specs/001-product-variants/
├── plan.md              # File này (/speckit.plan output)
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── products.md
│   └── categories.md
└── tasks.md             # Phase 2 output (/speckit.tasks — KHÔNG tạo bởi /speckit.plan)
```

### Mã nguồn (thư mục gốc repo)

```text
src/
├── MyProject.Domain/
│   ├── Entities/
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── ProductVariant.cs
│   │   ├── VariantAttribute.cs
│   │   └── VariantAttributeValue.cs
│   └── Repositories/
│       ├── ICategoryRepository.cs
│       └── IProductRepository.cs
│
├── MyProject.Application/
│   └── Features/
│       ├── Categories/
│       │   ├── CreateCategory/
│       │   ├── GetCategories/
│       │   └── DeleteCategory/
│       └── Products/
│           ├── Shared/
│           ├── CreateProduct/
│           ├── GetProducts/
│           ├── GetProductById/
│           ├── UpdateProduct/
│           ├── DeleteProduct/
│           └── ToggleProductStatus/
│
├── MyProject.Infrastructure/
│   ├── Data/
│   │   └── Configurations/
│   │       ├── CategoryConfiguration.cs
│   │       ├── ProductConfiguration.cs
│   │       ├── ProductVariantConfiguration.cs
│   │       ├── VariantAttributeConfiguration.cs
│   │       └── VariantAttributeValueConfiguration.cs
│   └── Repositories/
│       ├── CategoryRepository.cs
│       └── ProductRepository.cs
│
└── MyProject.API/
    └── Endpoints/
        ├── Categories/
        │   ├── CreateCategoryEndpoint.cs
        │   ├── GetCategoriesEndpoint.cs
        │   └── DeleteCategoryEndpoint.cs
        └── Products/
            ├── CreateProductEndpoint.cs
            ├── GetProductsEndpoint.cs
            ├── GetProductByIdEndpoint.cs
            ├── UpdateProductEndpoint.cs
            ├── DeleteProductEndpoint.cs
            └── ToggleProductStatusEndpoint.cs

tests/
├── MyProject.Domain.UnitTests/
│   └── Entities/
│       ├── ProductTests.cs
│       ├── ProductVariantTests.cs
│       └── CategoryTests.cs
├── MyProject.Application.UnitTests/
│   └── Features/
│       ├── Products/
│       └── Categories/
└── MyProject.API.IntegrationTests/
    └── Products/
        ├── CreateProductTests.cs
        ├── GetProductsTests.cs
        ├── UpdateProductTests.cs
        ├── DeleteProductTests.cs
        └── ToggleProductStatusTests.cs
```

**Quyết định cấu trúc**: Sử dụng cấu trúc Clean Architecture 4-layer đã có sẵn.
Không tạo project mới — thêm entities/features/endpoints vào đúng layer hiện có.

## Theo dõi độ phức tạp

> Không có vi phạm hiến pháp cần ghi nhận.
