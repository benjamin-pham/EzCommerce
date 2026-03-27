namespace MyProject.Domain.Abstractions;

public interface IRepository<T, TKey>
    where T : BaseEntity<TKey>
    where TKey : notnull
{
    Task<T?> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}

// Shorthand for the common case — Guid primary key
public interface IRepository<T> : IRepository<T, Guid>
    where T : BaseEntity
{
}
