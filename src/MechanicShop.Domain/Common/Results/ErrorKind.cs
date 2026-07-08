namespace MechanicShop.Domain.Common.Results;

public enum ErrorKind
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Unexpected,
    Failure,
}
