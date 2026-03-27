namespace MyProject.Domain.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
