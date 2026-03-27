namespace MyProject.Domain.Abstractions;

public abstract class BaseEntity<TKey> where TKey : notnull
{
    public TKey Id { get; init; } = default!;

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void SoftDelete() => IsDeleted = true;
}

public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}
