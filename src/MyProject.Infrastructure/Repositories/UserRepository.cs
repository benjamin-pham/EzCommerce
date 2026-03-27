using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;
using MyProject.Domain.Repositories;
using MyProject.Infrastructure.Data;

namespace MyProject.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        await Context.Users
            .FirstOrDefaultAsync(u => u.Username == username, ct);

    public new async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await Context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByHashedRefreshTokenAsync(string hashedToken, CancellationToken ct = default) =>
        await Context.Users
            .FirstOrDefaultAsync(u => u.HashedRefreshToken == hashedToken, ct);
}
