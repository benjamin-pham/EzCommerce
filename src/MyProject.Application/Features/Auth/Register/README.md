# Register User — Business Logic

## Mục đích

Cho phép người dùng mới tạo tài khoản bằng cách cung cấp thông tin cá nhân cơ bản.

## Luồng xử lý

1. `RegisterUserCommandValidator` kiểm tra đầu vào (FluentValidation → 400 nếu lỗi)
2. `RegisterUserCommandHandler` kiểm tra `username` chưa tồn tại trong DB → 409 nếu trùng
3. `IPasswordHasher.Hash(password)` tạo PBKDF2-SHA256 hash
4. `User.Create(...)` tạo entity mới (factory method duy nhất)
5. `IUserRepository.AddAsync(user)` → `IUnitOfWork.SaveChangesAsync()` lưu vào DB
6. Trả về `RegisterUserResponse` với `userId`, `username`, `firstName`, `lastName`

## Error codes

| Code | HTTP | Mô tả |
|------|------|-------|
| `User.UsernameAlreadyTaken` | 409 | Username đã được sử dụng |
| Validation errors | 400 | Dữ liệu đầu vào không hợp lệ |

## Validation rules

| Field | Rule |
|-------|------|
| `firstName` | Bắt buộc, max 100 ký tự |
| `lastName` | Bắt buộc, max 100 ký tự |
| `username` | Bắt buộc, 6–50 ký tự, chỉ `[a-zA-Z0-9]` |
| `password` | Bắt buộc, ≥8 ký tự, có chữ thường + chữ hoa + số |
| `email` | Tuỳ chọn, định dạng email hợp lệ nếu có |
| `phone` | Tuỳ chọn, 7–15 ký tự `[0-9+]` nếu có |
| `birthday` | Tuỳ chọn, ≤ ngày hiện tại nếu có |

## Security notes

- Password không bao giờ được lưu dạng plain text
- Password không bao giờ được trả về trong response
- Username là case-sensitive
