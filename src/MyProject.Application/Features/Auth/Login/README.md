# Login — Business Logic

## Mục đích

User Story 2: Người dùng có tài khoản đăng nhập và nhận access token (24h) + refresh token (7 ngày).

## Flow

1. Nhận `LoginUserCommand` với `username` và `password`.
2. Tìm user theo `username` trong database.
3. Nếu không tìm thấy user hoặc password sai → trả về `User.InvalidCredentials` (không tiết lộ trường nào sai — FR-007).
4. Generate `accessToken` (JWT, 24h) và `refreshToken` (ngẫu nhiên, 7 ngày).
5. Hash refresh token và lưu vào user qua `User.SetRefreshToken(...)`.
6. Lưu thay đổi và trả về `LoginUserResponse`.

## Security

- Thông báo lỗi chung cho cả username và password sai (FR-007) — tránh username enumeration.
- Refresh token lưu dạng hash SHA-256 trong database, không lưu plaintext.
- Không log password, token thô, hay PII (SC-007 / Constitution V).

## Logging

- `Information`: Đăng nhập thành công — log `{UserId}`, `{Timestamp}`.
- `Warning`: Đăng nhập thất bại — log `{Username}`, `{Reason}`, `{Timestamp}`.

## Error Codes

| Code | HTTP | Mô tả |
|------|------|-------|
| `User.InvalidCredentials` | 401 | Username hoặc password không đúng |
