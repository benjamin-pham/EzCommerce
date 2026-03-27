using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Abstractions;
using MyProject.Infrastructure.Data;

namespace MyProject.Infrastructure.Repositories;

public abstract class Repository<T, TKey>(AppDbContext context) : IRepository<T, TKey>
    where T : BaseEntity<TKey>
    where TKey : notnull
{
    protected readonly AppDbContext Context = context;

    public async Task<T?> GetByIdAsync(TKey id, CancellationToken ct = default) =>
        await Context.Set<T>().FindAsync([id!], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await Context.Set<T>().ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Context.Set<T>().AddAsync(entity, ct);

    public void Update(T entity) =>
        Context.Set<T>().Update(entity);

    public void Remove(T entity) =>
        Context.Set<T>().Remove(entity);
}

// Shorthand for the common case — Guid primary key
public abstract class Repository<T>(AppDbContext context) : Repository<T, Guid>(context)
    where T : BaseEntity
{
}
