namespace MechanicShop.Domain.Common.Results;

public readonly record struct Error
{
    private Error(string code, string description, ErrorKind type)
    {
        Code = code;
        Type = type;
        Description = description;
    }

    public string Code { get; }
    public string Description { get; }
    public ErrorKind Type { get; }

    public static Error Validation(
        string code = nameof(Validation),
        string description = "Validation error"
    ) => new(code, description, ErrorKind.Validation);

    public static Error NotFound(
        string code = nameof(NotFound),
        string description = "Resource not found"
    ) => new(code, description, ErrorKind.NotFound);

    public static Error Conflict(
        string code = nameof(Conflict),
        string description = "Resource conflict"
    ) => new(code, description, ErrorKind.Conflict);

    public static Error Unauthorized(
        string code = nameof(Unauthorized),
        string description = "Unauthorized") => new(code, description, ErrorKind.Unauthorized);

    public static Error Forbidden(
        string code = nameof(Forbidden),
        string description = "Forbidden"
    ) => new(code, description, ErrorKind.Forbidden);

    public static Error Unexpected(
        string code = nameof(Unexpected),
        string description = "Unexpected error"
    ) => new(code, description, ErrorKind.Unexpected);

    public static Error Failure(
        string code = nameof(Failure),
        string description = "Operation failed"
    ) => new(code, description, ErrorKind.Failure);

    public static Error Create(string code, string description, ErrorKind type) =>
        new(code, description, type);
}
