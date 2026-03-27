# GetProfile — Xem thông tin hồ sơ cá nhân

## Mục đích

Cho phép người dùng đã đăng nhập xem toàn bộ thông tin hồ sơ của mình.

## Endpoint

`GET /api/auth/me` — yêu cầu Bearer token hợp lệ.

## Flow

1. Endpoint lấy `userId` từ JWT claim `sub`.
2. `GetProfileQuery(userId)` được gửi tới handler qua MediatR.
3. Handler dùng Dapper để query bảng `users` theo `id`, chỉ SELECT các cột cần thiết (không SELECT `password_hash`).
4. Trả về 200 OK với `UserProfileResponse`; trả về 404 nếu user không tồn tại.

## Bảo mật

- `password_hash` và token không bao giờ được trả về trong response (FR-010).
- Endpoint được bảo vệ bởi JWT Bearer authentication (`RequireAuthorization()`).
