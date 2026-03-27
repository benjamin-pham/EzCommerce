# API Contract: Products

**Base URL**: `/api/products`
**Auth**: Bearer token (admin role required cho tất cả endpoints)

---

## POST /api/products — Tạo sản phẩm mới

### Request Body

```json
{
  "name": "Áo thun unisex",
  "sku": "AT-001",
  "slug": "ao-thun-unisex",
  "description": "Áo thun cotton 100%",
  "categoryIds": ["uuid-1", "uuid-2"],
  "variantAttributes": [
    {
      "name": "Màu sắc",
      "values": ["Đỏ", "Xanh", "Trắng"]
    },
    {
      "name": "Kích thước",
      "values": ["S", "M", "L"]
    }
  ],
  "variants": [
    { "sku": "AT-001-DO-S",  "attributeValues": [{ "attributeName": "Màu sắc", "value": "Đỏ" }, { "attributeName": "Kích thước", "value": "S" }],  "isActive": true },
    { "sku": "AT-001-DO-M",  "attributeValues": [{ "attributeName": "Màu sắc", "value": "Đỏ" }, { "attributeName": "Kích thước", "value": "M" }],  "isActive": true }
  ]
}
```

> **Ghi chú**: `slug` là optional — nếu bỏ trống, hệ thống tự sinh từ `name`.
> `variantAttributes` + `variants` là optional — sản phẩm có thể không có biến thể.
> Mảng `variants` trong request là danh sách biến thể admin chọn giữ lại (có thể ít hơn
> tổ hợp đầy đủ nếu admin đã xóa bớt ở UI trước khi submit).

### Validation Rules

| Trường | Quy tắc |
|--------|---------|
| `name` | Required, max 500 ký tự |
| `sku` | Required, max 100 ký tự, duy nhất toàn hệ thống |
| `slug` | Max 600 ký tự, duy nhất toàn hệ thống (nếu cung cấp) |
| `categoryIds` | Required, mảng ≥ 1 phần tử, các ID phải tồn tại |
| `variantAttributes[].name` | Required nếu có attributes |
| `variantAttributes[].values` | Required, mảng ≥ 1 |
| `variants[].sku` | Required, duy nhất toàn hệ thống, không trùng với product SKU |

### Response: 201 Created

```json
{
  "id": "uuid",
  "name": "Áo thun unisex",
  "sku": "AT-001",
  "slug": "ao-thun-unisex",
  "isActive": true,
  "variantCount": 2
}
```

### Response: 400 Bad Request (validation failure)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "sku": ["SKU 'AT-001' đã tồn tại trong hệ thống."],
    "slug": ["Slug 'ao-thun-unisex' đã được sử dụng. Gợi ý: 'ao-thun-unisex-2'."]
  }
}
```

---

## GET /api/products — Danh sách sản phẩm (có phân trang)

### Query Parameters

| Tham số | Kiểu | Mặc định | Mô tả |
|---------|------|---------|-------|
| `page` | int | 1 | Trang hiện tại |
| `pageSize` | int | 20 | Số bản ghi mỗi trang (max: 100) |
| `search` | string | - | Tìm theo tên hoặc SKU |
| `categoryId` | uuid | - | Lọc theo danh mục |
| `isActive` | bool | - | Lọc theo trạng thái |

### Response: 200 OK

```json
{
  "items": [
    {
      "id": "uuid",
      "name": "Áo thun unisex",
      "sku": "AT-001",
      "slug": "ao-thun-unisex",
      "isActive": true,
      "variantCount": 6,
      "categories": [
        { "id": "uuid-1", "name": "Áo" }
      ]
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

---

## GET /api/products/{id} — Chi tiết sản phẩm

### Response: 200 OK

```json
{
  "id": "uuid",
  "name": "Áo thun unisex",
  "sku": "AT-001",
  "slug": "ao-thun-unisex",
  "description": "Áo thun cotton 100%",
  "isActive": true,
  "categories": [
    { "id": "uuid-1", "name": "Áo" }
  ],
  "variantAttributes": [
    {
      "id": "uuid",
      "name": "Màu sắc",
      "values": [
        { "id": "uuid", "value": "Đỏ" },
        { "id": "uuid", "value": "Xanh" }
      ]
    }
  ],
  "variants": [
    {
      "id": "uuid",
      "sku": "AT-001-DO-S",
      "isActive": true,
      "attributeValues": [
        { "attributeName": "Màu sắc", "value": "Đỏ" },
        { "attributeName": "Kích thước", "value": "S" }
      ]
    }
  ]
}
```

### Response: 404 Not Found

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Sản phẩm với ID 'uuid' không tồn tại."
}
```

---

## PUT /api/products/{id} — Cập nhật sản phẩm

### Request Body

```json
{
  "name": "Áo thun unisex mới",
  "slug": "ao-thun-unisex-moi",
  "description": "Mô tả cập nhật",
  "categoryIds": ["uuid-1"],
  "variantsToAdd": [
    { "sku": "AT-001-VANG-S", "attributeValueIds": ["uuid-vang", "uuid-s"] }
  ],
  "variantIdsToRemove": ["uuid-variant-cu"],
  "variantIdsToDeactivate": ["uuid-variant-khong-dung"]
}
```

### Response: 200 OK

```json
{
  "id": "uuid",
  "name": "Áo thun unisex mới",
  "slug": "ao-thun-unisex-moi",
  "isActive": true
}
```

---

## DELETE /api/products/{id} — Xóa sản phẩm (soft delete)

### Response: 204 No Content

Xóa mềm sản phẩm và tất cả biến thể liên quan.

### Response: 404 Not Found

```json
{
  "detail": "Sản phẩm với ID 'uuid' không tồn tại."
}
```

---

## PATCH /api/products/{id}/status — Toggle trạng thái hoạt động

### Response: 200 OK

```json
{
  "id": "uuid",
  "isActive": false
}
```
