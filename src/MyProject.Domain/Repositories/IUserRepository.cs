using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;

namespace MyProject.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    new Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByHashedRefreshTokenAsync(string hashedToken, CancellationToken ct = default);
}
