# Nghiên cứu kỹ thuật: Quản lý sản phẩm có biến thể

**Tính năng**: `001-product-variants` | **Ngày**: 2026-03-27

---

## 1. Sinh tổ hợp biến thể (Cartesian Product)

**Quyết định**: Thực hiện logic sinh tổ hợp trong Application handler (`CreateProductCommandHandler`),
không đặt trong domain entity — domain chỉ nhận danh sách `ProductVariant` đã hoàn chỉnh và
validate từng variant.

**Lý do**: Logic sinh tổ hợp là orchestration (dùng LINQ `SelectMany` lồng nhau), không phải
business rule của entity. Entity `Product` chịu trách nhiệm validate SKU duy nhất và trạng thái;
handler chịu trách nhiệm sinh tổ hợp từ input của admin.

**Thay thế đã xem xét**: Đặt trong domain service — bị bác vì tạo thêm abstraction không cần thiết
khi handler đã đủ điều kiện làm việc này.

**Cách triển khai**:
```csharp
// Trong CreateProductCommandHandler
var combinations = command.VariantAttributes
    .Select(a => a.Values.Select(v => (AttributeName: a.Name, Value: v)))
    .Aggregate(
        seed: new List<List<(string, string)>> { [] },
        (acc, values) => acc
            .SelectMany(combo => values.Select(v => combo.Append(v).ToList()))
            .ToList());
```

---

## 2. Quan hệ nhiều-nhiều Product ↔ Category (EF Core)

**Quyết định**: Dùng skip navigation (EF Core 5+) với join entity ẩn.
Không cần class `ProductCategory` trong Domain.

**Lý do**: Quan hệ thuần túy không có payload (không có extra fields trên join table),
skip navigation đủ dùng và đơn giản hơn.

**Cấu hình Fluent API**:
```csharp
// Trong ProductConfiguration
builder.HasMany(p => p.Categories)
    .WithMany(c => c.Products)
    .UsingEntity(j => j.ToTable("product_categories"));
```

**Thay thế đã xem xét**: Explicit join entity `ProductCategory` — bị bác vì không có payload,
thêm class không cần thiết.

---

## 3. Quan hệ nhiều-nhiều ProductVariant ↔ VariantAttributeValue

**Quyết định**: Dùng skip navigation tương tự — join table `product_variant_attribute_values`.

**Lý do**: Mỗi `ProductVariant` xác định bởi một tổ hợp `VariantAttributeValue` — không có payload
trên join table.

**Cấu hình Fluent API**:
```csharp
// Trong ProductVariantConfiguration
builder.HasMany(v => v.AttributeValues)
    .WithMany()
    .UsingEntity(j => j.ToTable("product_variant_attribute_values"));
```

---

## 4. SKU duy nhất toàn hệ thống

**Quyết định**: Validate SKU trùng ở Application layer qua repository query trước khi persist.
Thêm unique index trên cột `sku` ở cả bảng `products` và `product_variants` trong EF Core config.
Xử lý `DbUpdateException` ở Infrastructure để map về `Result.Failure` với error rõ ràng.

**Lý do**: Unique index là tầng bảo vệ cuối cùng tại DB. Application layer check trước để
trả error message thân thiện mà không cần catch exception thô.

**Cách triển khai trong Domain**:
```csharp
// IProductRepository
Task<bool> IsSkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default);
```

**Thay thế đã xem xét**: Chỉ dùng DB unique index — bị bác vì message lỗi không thân thiện
và không biết SKU nào trùng.

---

## 5. Slug duy nhất — sinh và kiểm tra

**Quyết định**: Slug được sinh từ tên sản phẩm (slugify) tại Application handler nếu admin
không cung cấp. Kiểm tra trùng slug qua repository. **Hệ thống không tự thêm suffix** — nếu
trùng thì từ chối và gợi ý (trả về `suggestedSlug` trong error response).

**Thuật toán slugify**: Lowercase, bỏ dấu tiếng Việt (NFD normalize + remove combining chars),
replace space/special chars bằng `-`, remove leading/trailing `-`.

**Thư viện**: Không dùng thư viện ngoài — tự implement `SlugHelper` đơn giản trong Application
(static utility, không phải service) để tránh dependency mới.

**Gợi ý slug thay thế**: Thêm số tăng dần (`san-pham-ao-thun-2`) vào response error để admin
tự chỉnh, không tự lưu.

---

## 6. Soft delete cascade (Product → Variants)

**Quyết định**: Khi `Product.SoftDelete()` được gọi (từ `DeleteProductCommandHandler`),
handler cũng gọi `SoftDelete()` trên tất cả `ProductVariant` của sản phẩm đó trong cùng transaction.

**Lý do**: EF Core không hỗ trợ cascade soft delete tự động. `BaseEntity.SoftDelete()` đã có sẵn —
handler chịu trách nhiệm xử lý cascade thủ công trong cùng `UnitOfWork`.

**Thay thế đã xem xét**: DB trigger — bị bác vì vi phạm nguyên tắc "business logic không ở DB".

---

## 7. Chặn xóa Category khi còn sản phẩm liên kết

**Quyết định**: `DeleteCategoryCommandHandler` query số sản phẩm thuộc category đó trước khi xóa.
Nếu count > 0 → `Result.Failure(CategoryErrors.HasProducts(count))`.

**Query dùng Dapper** (CQRS read):
```sql
SELECT COUNT(*) FROM product_categories WHERE category_id = @categoryId
AND EXISTS (SELECT 1 FROM products WHERE id = product_id AND is_deleted = false)
```

---

## 8. Phân trang và tìm kiếm sản phẩm

**Quyết định**: `GetProductsQuery` nhận `Page`, `PageSize`, `Search` (tìm theo tên/SKU), `CategoryId`.
Dùng **Dapper** với raw SQL để đảm bảo hiệu năng (không dùng EF Core cho query).

**Index PostgreSQL cần tạo**:
- `products(sku)` — UNIQUE
- `products(slug)` — UNIQUE
- `products(name)` — GIN index (`pg_trgm`) để tìm kiếm full-text
- `product_variants(sku)` — UNIQUE
- `product_categories(category_id)` — để lọc theo danh mục

**Thay thế đã xem xét**: EF Core LINQ query — bị bác vì khó kiểm soát SQL sinh ra,
đặc biệt khi JOIN nhiều bảng.
