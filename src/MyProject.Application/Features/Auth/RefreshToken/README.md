# Refresh Token — Business Logic

## Mục đích

User Story 2: Người dùng dùng refresh token hợp lệ để lấy access token mới mà không cần đăng nhập lại.

## Flow

1. Nhận `RefreshTokenCommand` với `refreshToken` thô.
2. Hash token và tìm user theo `hashed_refresh_token` trong database.
3. Kiểm tra token có tồn tại và chưa hết hạn.
4. Nếu không hợp lệ hoặc hết hạn → trả về `User.InvalidRefreshToken` (401).
5. Generate access token mới + refresh token mới (rotating refresh token).
6. Hash refresh token mới, lưu vào user qua `User.SetRefreshToken(...)` — vô hiệu hoá token cũ.
7. Lưu thay đổi và trả về `LoginUserResponse` với token mới.

## Security

- **Rotating refresh token**: Mỗi lần refresh cấp token mới, token cũ bị vô hiệu hoá ngay lập tức.
- Refresh token lưu dạng hash SHA-256, không lưu plaintext.
- Không log token thô hay PII.

## Logging

- `Information`: Refresh thành công — log `{UserId}`, `{Timestamp}`.

## Error Codes

| Code | HTTP | Mô tả |
|------|------|-------|
| `User.InvalidRefreshToken` | 401 | Token không hợp lệ hoặc đã hết hạn |
