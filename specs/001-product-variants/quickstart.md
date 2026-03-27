# Hướng dẫn nhanh: Quản lý sản phẩm có biến thể

**Tính năng**: `001-product-variants` | **Nhánh**: `001-product-variants`

---

## Yêu cầu môi trường

- .NET 10 SDK
- Docker (để chạy PostgreSQL và test Testcontainers)
- API key hợp lệ (xem `.env` hoặc `appsettings.Development.json`)

---

## Chạy project

```bash
# Từ thư mục gốc repo
cd c:/CN-ManPM/project/EzCommerce

# Khởi động PostgreSQL (nếu dùng Docker Compose)
docker compose up -d db

# Chạy migration (sau khi code entities và configurations đã tạo xong)
dotnet ef database update --project src/MyProject.Infrastructure --startup-project src/MyProject.API

# Chạy API
dotnet run --project src/MyProject.API
```

---

## Luồng sử dụng cơ bản

### 1. Tạo danh mục

```http
POST http://localhost:5000/api/categories
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Áo nam",
  "description": "Tất cả các loại áo dành cho nam giới"
}
```

### 2. Tạo sản phẩm đơn giản (không biến thể)

```http
POST http://localhost:5000/api/products
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Áo polo trắng",
  "sku": "POLO-001",
  "categoryIds": ["{category-id}"]
}
```

### 3. Tạo sản phẩm có biến thể

```http
POST http://localhost:5000/api/products
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Áo thun unisex",
  "sku": "AT-001",
  "categoryIds": ["{category-id}"],
  "variantAttributes": [
    { "name": "Màu sắc", "values": ["Đỏ", "Xanh"] },
    { "name": "Kích thước", "values": ["S", "M"] }
  ],
  "variants": [
    { "sku": "AT-001-DO-S",   "attributeValues": [{"attributeName": "Màu sắc", "value": "Đỏ"}, {"attributeName": "Kích thước", "value": "S"}] },
    { "sku": "AT-001-DO-M",   "attributeValues": [{"attributeName": "Màu sắc", "value": "Đỏ"}, {"attributeName": "Kích thước", "value": "M"}] },
    { "sku": "AT-001-XANH-S", "attributeValues": [{"attributeName": "Màu sắc", "value": "Xanh"}, {"attributeName": "Kích thước", "value": "S"}] },
    { "sku": "AT-001-XANH-M", "attributeValues": [{"attributeName": "Màu sắc", "value": "Xanh"}, {"attributeName": "Kích thước", "value": "M"}] }
  ]
}
```

### 4. Xem danh sách sản phẩm

```http
GET http://localhost:5000/api/products?page=1&pageSize=20&search=áo&categoryId={id}
Authorization: Bearer {token}
```

### 5. Toggle trạng thái sản phẩm

```http
PATCH http://localhost:5000/api/products/{id}/status
Authorization: Bearer {token}
```

---

## Chạy tests

```bash
# Unit tests
dotnet test tests/MyProject.Domain.UnitTests
dotnet test tests/MyProject.Application.UnitTests

# Integration tests (cần Docker)
dotnet test tests/MyProject.API.IntegrationTests
```

---

## Lưu ý quan trọng

- **SKU duy nhất toàn hệ thống**: SKU của sản phẩm và SKU của biến thể dùng cùng không gian duy nhất.
  Không thể có `sku = "AT-001"` vừa là SKU sản phẩm vừa là SKU biến thể.
- **Slug tự động sinh**: Nếu không cung cấp `slug`, hệ thống tự sinh từ `name` (bỏ dấu, lowercase, dùng `-`).
  Nếu slug trùng, hệ thống từ chối và gợi ý slug thay thế — admin phải tự chỉnh.
- **Xóa danh mục bị chặn**: Không thể xóa danh mục nếu có bất kỳ sản phẩm nào đang thuộc danh mục đó.
- **Soft delete cascade**: Xóa sản phẩm sẽ đánh dấu đã xóa cả tất cả biến thể liên quan.
- **Biến thể không bắt buộc**: Sản phẩm có thể tồn tại mà không có biến thể nào.

---

## Artifacts thiết kế

| File | Mô tả |
|------|-------|
| [spec.md](spec.md) | Đặc tả tính năng (user stories, acceptance criteria) |
| [plan.md](plan.md) | Kế hoạch triển khai (bối cảnh kỹ thuật, cấu trúc) |
| [research.md](research.md) | Nghiên cứu kỹ thuật (quyết định + lý do) |
| [data-model.md](data-model.md) | Mô hình dữ liệu (entities, DB schema) |
| [contracts/products.md](contracts/products.md) | API contract cho Products |
| [contracts/categories.md](contracts/categories.md) | API contract cho Categories |
| [tasks.md](tasks.md) | Danh sách tasks (tạo bởi `/speckit.tasks`) |
