using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using MechanicShop.Client.Models;
using MechanicShop.Contracts.Requests.Customers;

namespace MechanicShop.Client.Services;

public class ServiceApi(IHttpClientFactory httpClientFactory, TimeZoneService timeZoneService)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("MechanicShopClient");
    private readonly TimeZoneService _timeZoneService = timeZoneService;

    #region Customers
    public async Task<ApiResult<List<CustomerModel>>> GetCustomersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/customers");

            if (response.IsSuccessStatusCode)
            {
                var customers = await response.Content.ReadFromJsonAsync<List<CustomerModel>>();
                return ApiResult<List<CustomerModel>>.Success(customers ?? []);
            }

            return await HandleErrorResponseAsync<List<CustomerModel>>(response);

        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<List<CustomerModel>>(ex, "Failed to get customers.");
        }


    }

    public async Task<ApiResult<CustomerModel>> GetCustomerAsync(Guid customerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/customers/{customerId}");

            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadFromJsonAsync<CustomerModel>();

                if (customer is null)
                {
                    return ApiResult<CustomerModel>.Failure("Failed to get customer.");
                }

                return ApiResult<CustomerModel>.Success(customer!);
            }

            return await HandleErrorResponseAsync<CustomerModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<CustomerModel>(ex, $"Failed to get customer {customerId}.");
        }
    }

    public async Task<ApiResult<CustomerModel>> CreateCustomerAsync(CreateCustomerRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/customers", request);

            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadFromJsonAsync<CustomerModel>();

                if (customer is null)
                {
                    return ApiResult<CustomerModel>.Failure("Failed to create customer.");
                }

                return ApiResult<CustomerModel>.Success(customer!);
            }

            return await HandleErrorResponseAsync<CustomerModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<CustomerModel>(ex, "Failed to create customer.");
        }
    }

    public async Task<ApiResult<CustomerModel>> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/customers/{customerId}", request);

            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadFromJsonAsync<CustomerModel>();

                if (customer is null)
                {
                    return ApiResult<CustomerModel>.Failure("Failed to update customer.");
                }

                return ApiResult<CustomerModel>.Success(customer!);
            }

            return await HandleErrorResponseAsync<CustomerModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<CustomerModel>(ex, $"Failed to update customer {customerId}.");
        }
    }

    public async Task<ApiResult> DeleteCustomerAsync(Guid customerId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/customers/{customerId}");

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to delete customer {customerId}.");
        }
    }

    #endregion


    // Private Helper Methods

    private static async Task<ApiResult<T>> HandleErrorResponseAsync<T>(
        HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        try
        {
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(
                content,
                options: new() { PropertyNameCaseInsensitive = true });

            if (problemDetails is not null)
            {
                return ApiResult<T>.Failure(
                    problemDetails.Title ?? "Error",
                    problemDetails.Detail ?? "An error occurred",
                    problemDetails.Status ?? (int)response.StatusCode,
                    problemDetails.Errors);
            }

            return ApiResult<T>.Failure(
                GetFriendlyErrorMessage(response.StatusCode),
                content,
                (int)response.StatusCode);
        }
        catch (JsonException)
        {
            return ApiResult<T>.Failure(
                GetFriendlyErrorMessage(response.StatusCode),
                content,
                statusCode: (int)response.StatusCode);
        }
    }

    private static Task<ApiResult> HandleErrorResponseAsync(HttpResponseMessage response)
    {
        return HandleErrorResponseAsync<object>(response)
            .ContinueWith(static t =>
                ApiResult.Failure(
                    t.Result.ErrorMessage,
                    t.Result.ErrorDetails,
                    t.Result.StatusCode,
                    t.Result.ValidationErrors));
    }

    private static Task<ApiResult<T>> HandleExceptionAsync<T>(Exception ex, string message) =>
        Task.FromResult(
            ex switch
            {
                HttpRequestException => ApiResult<T>.Failure($"Network error occurred. {message}"),
                TaskCanceledException => ApiResult<T>.Failure($"Request timed out. {message}"),
                _ => ApiResult<T>.Failure($"An unexpected error occurred. {message}"),
            });

    private static Task<ApiResult> HandleExceptionAsync(Exception ex, string message) =>
        HandleExceptionAsync<object>(ex, message)
            .ContinueWith(t =>
                ApiResult.Failure(
                    t.Result.ErrorMessage,
                    t.Result.ErrorDetails,
                    t.Result.StatusCode,
                    t.Result.ValidationErrors));

    private static string GetFriendlyErrorMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Invalid request. Please check your input and try again.",
            HttpStatusCode.Unauthorized => "You are not authorized to perform this action.",
            HttpStatusCode.Forbidden => "You don't have permission to perform this action.",
            HttpStatusCode.NotFound => "The requested resource was not found.",
            HttpStatusCode.Conflict =>
                "The operation conflicts with the current state of the resource.",
            HttpStatusCode.UnprocessableEntity => "The request contains invalid data.",
            HttpStatusCode.InternalServerError =>
                "A server error occurred. Please try again later.",
            HttpStatusCode.BadGateway => "Service temporarily unavailable. Please try again later.",
            HttpStatusCode.ServiceUnavailable =>
                "Service temporarily unavailable. Please try again later.",
            HttpStatusCode.GatewayTimeout => "The request timed out. Please try again.",
            _ => "An error occurred while processing your request.",
        };
    }
}
