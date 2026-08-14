namespace MechanicShop.Client.Services;

public class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorDetails { get; set; }
    public int StatusCode { get; set; }

    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    public string? FirstErrorMessage => ValidationErrors?.SelectMany(kvp => kvp.Value).FirstOrDefault() ?? ErrorMessage;

    public static ApiResult<T> Success(T data)
    {
        return new ApiResult<T> { IsSuccess = true, Data = data };
    }

    public static ApiResult<T> Failure(
        string? errorMessage,
        string? errorDetails = null,
        int statusCode = 0,
        Dictionary<string, string[]>? validationErrors = null)
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails,
            StatusCode = statusCode,
            ValidationErrors = validationErrors
        };
    }
}

public class ApiResult : ApiResult<object>
{
    public static ApiResult Success() => new() { IsSuccess = true };

    public static new ApiResult Failure(
            string? message,
            string? detail = null,
            int statusCode = 0,
            Dictionary<string, string[]>? validationErrors = null)
    {
        return new ApiResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorDetails = detail,
            StatusCode = statusCode,
            ValidationErrors = validationErrors,
        };
    }
}