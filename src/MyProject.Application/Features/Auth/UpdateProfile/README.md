# UpdateProfile — Business Logic

## Purpose

Allows an authenticated user to update their personal profile information via `PUT /api/auth/me`.

## What can be updated

- `firstName` (required, max 100 chars)
- `lastName` (required, max 100 chars)
- `email` (optional, must be valid format if provided)
- `phone` (optional, 7–15 chars, digits and `+` only)
- `birthday` (optional, must not be in the future)

## What cannot be changed via this endpoint

- `username` — immutable after registration (FR-014)
- `password` — changed via a dedicated change-password flow

## Flow

1. Endpoint extracts `userId` from JWT `sub` claim.
2. `UpdateProfileCommand` is dispatched via MediatR.
3. Handler loads the `User` entity from `IUserRepository`.
4. Calls `User.UpdateProfile(...)` — domain method enforces invariants.
5. Persists via `IUnitOfWork.SaveChangesAsync`.
6. Returns `204 No Content` on success.

## Error cases

| Condition | HTTP status |
|-----------|-------------|
| Missing/invalid Bearer token | 401 Unauthorized |
| Blank firstName or lastName | 400 Bad Request (FluentValidation) |
| Invalid email format | 400 Bad Request |
| Phone not matching `[0-9+]{7,15}` | 400 Bad Request |
| Birthday in the future | 400 Bad Request |
| User not found (edge case) | 404 Not Found |

## Security notes

- Endpoint requires `[Authorize]` — unauthenticated requests are rejected at middleware level.
- `username` is never read from the request body; only profile fields are accepted (FR-014).
