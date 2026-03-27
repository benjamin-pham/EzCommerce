namespace MyProject.Domain.Abstractions;

public record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("General.Null", "A null value was provided.");

    public static Error NotFound(string entity, object id) =>
        new($"{entity}.NotFound", $"The {entity} with Id '{id}' was not found.");

    public static Error Conflict(string entity) =>
        new($"{entity}.Conflict", $"The {entity} already exists.");

    public static Error Validation(string message) =>
        new("General.Validation", message);
}
