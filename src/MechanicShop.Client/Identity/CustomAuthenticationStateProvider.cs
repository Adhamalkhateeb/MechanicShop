using System.Security.Claims;
using System.Text.Json;

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components.Authorization;

namespace MechanicShop.Client.Identity;

public class CustomAuthenticationStateProvider(
    ILocalStorageService localStorageService,
    IHttpClientFactory httpClientFactory,
    ILogger<CustomAuthenticationStateProvider> logger) : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorageService = localStorageService;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger = logger;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly ClaimsPrincipal unauthenticated = new(new ClaimsIdentity());
    private bool authenticated = false;
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
                var identity = new ClaimsIdentity(userInfo.Claims, nameof(CustomAuthenticationStateProvider));
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
}

