using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace MechanicShop.Client.Identity;

public class CustomAuthenticationStateProvider(
    ILocalStorageService localStorageService,
    IHttpClientFactory httpClientFactory,
    ILogger<CustomAuthenticationStateProvider> logger
) : AuthenticationStateProvider, IAccountManagement
{
    private readonly ILocalStorageService _localStorageService = localStorageService;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger = logger;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    private readonly ClaimsPrincipal unauthenticated = new(new ClaimsIdentity());
    private bool authenticated = false;

    public async Task<FormResult> LoginAsync(string email, string password)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("MechanicShopClient");
            var result = await httpClient.PostAsJsonAsync(
                "identity/token/generate",
                new { email, password }
            );

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadFromJsonAsync<TokenResponse>();
                await _localStorageService.SetItemAsync("authResult", response);

                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                return new FormResult { Succeeded = true };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
        }

        return new FormResult { Succeeded = false, ErrorList = ["Invalid email and/or password."] };
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        authenticated = false;
        var user = unauthenticated;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("MechanicShopClient");

            var userResponse = await httpClient.GetAsync("identity/current-user/claims");

            userResponse.EnsureSuccessStatusCode();

            var userJson = await userResponse.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<UserInfo>(userJson, _jsonSerializerOptions);

            if (userInfo is not null)
            {
                var identity = new ClaimsIdentity(
                    userInfo.Claims,
                    nameof(CustomAuthenticationStateProvider)
                );
                user = new ClaimsPrincipal(identity);
                authenticated = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user");
        }

        return new AuthenticationState(user);
    }

    public async Task<bool> CheckAuthenticatedAsync()
    {
        await GetAuthenticationStateAsync();
        return authenticated;
    }

    public async Task<TokenResponse?> LoadAccessTokenFromStorageAsync()
    {
        return await _localStorageService.GetItemAsync<TokenResponse>("authResult");
    }

    public async Task LogoutAsync()
    {
        await _localStorageService.RemoveItemAsync("authResult");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<TokenResponse?> RefreshTokenAsync()
    {
        var authResult = await _localStorageService.GetItemAsync<TokenResponse>("authResult");

        if (authResult?.RefreshToken is null)
        {
            return null; // No refresh token available
        }

        var httpClient = _httpClientFactory.CreateClient("MechanicShopClient");
        var refreshResponse = await httpClient.PostAsJsonAsync(
            "identity/token/refresh-token",
            new { ExpiredAccessToken = authResult.AccessToken, authResult.RefreshToken }
        );

        if (!refreshResponse.IsSuccessStatusCode)
        {
            return null; // Refresh failed
        }

        var newTokenResponse = await refreshResponse.Content.ReadFromJsonAsync<TokenResponse>();

        if (newTokenResponse is null || newTokenResponse.ExpiresOnUtc <= DateTime.UtcNow)
        {
            return null; // Avoid storing expired tokens
        }

        await _localStorageService.SetItemAsync("authResult", newTokenResponse);
        return newTokenResponse;
    }
}
