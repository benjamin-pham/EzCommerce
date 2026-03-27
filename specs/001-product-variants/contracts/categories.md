# API Contract: Categories

**Base URL**: `/api/categories`
**Auth**: Bearer token (admin role required cho tất cả endpoints)

---

## POST /api/categories — Tạo danh mục mới

### Request Body

```json
{
  "name": "Áo nam",
  "description": "Tất cả các loại áo dành cho nam giới"
}
```

### Validation Rules

| Trường | Quy tắc |
|--------|---------|
| `name` | Required, max 200 ký tự |
| `description` | Optional, max 1000 ký tự |

### Response: 201 Created

```json
{
  "id": "uuid",
  "name": "Áo nam",
  "description": "Tất cả các loại áo dành cho nam giới"
}
```

### Response: 400 Bad Request

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "name": ["Tên danh mục là bắt buộc."]
  }
}
```

---

## GET /api/categories — Danh sách tất cả danh mục

Trả về toàn bộ danh mục (không phân trang — danh mục phẳng, số lượng nhỏ).

### Response: 200 OK

```json
[
  {
    "id": "uuid-1",
    "name": "Áo nam",
    "description": "Tất cả các loại áo dành cho nam giới"
  },
  {
    "id": "uuid-2",
    "name": "Quần nam",
    "description": null
  }
]
```

---

## DELETE /api/categories/{id} — Xóa danh mục

Xóa mềm danh mục. **Bị chặn** nếu có bất kỳ sản phẩm nào đang thuộc danh mục này.

### Response: 204 No Content

Xóa thành công.

### Response: 409 Conflict (có sản phẩm liên kết)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Không thể xóa danh mục. Hiện có 12 sản phẩm đang thuộc danh mục này."
}
```

### Response: 404 Not Found

```json
{
  "detail": "Danh mục với ID 'uuid' không tồn tại."
}
```
