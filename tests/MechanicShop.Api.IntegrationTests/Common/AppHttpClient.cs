using System.Net.Http.Headers;
using System.Net.Http.Json;
using MechanicShop.Infrastructure.Identity;

namespace MechanicShop.Api.IntegrationTests.Common;

public class AppHttpClient
{
    private readonly HttpClient _httpClient;

    public AppHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetAuthorizationHeader(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token);
    }

    public void ClearAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<HttpResponseMessage> GetAsync(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync(requestUri, cancellationToken);
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.SendAsync(request, cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(
        string requestUri,
        T value,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsJsonAsync(requestUri, value, cancellationToken);
    }

    public async Task<HttpResponseMessage> PutAsJsonAsync<T>(
        string requestUri,
        T value,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PutAsJsonAsync(requestUri, value, cancellationToken);
    }

    public async Task<HttpResponseMessage> DeleteAsync(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.DeleteAsync(requestUri, cancellationToken);
    }

    public async Task<HttpResponseMessage> PatchAsJsonAsync<T>(
        string requestUri,
        T value,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PatchAsJsonAsync(requestUri, value, cancellationToken);
    }

    public async Task<T?> GetFromJsonAsync<T>(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    public async Task<T?> PostAndGetFromJsonAsync<TRequest, T>(
        string requestUri,
        TRequest value,
        CancellationToken cancellationToken = default)
    {
        var response = await PostAsJsonAsync(requestUri, value, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
