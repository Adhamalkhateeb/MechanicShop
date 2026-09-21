using Blazored.LocalStorage;

using MechanicShop.Client;
using MechanicShop.Client.Identity;
using MechanicShop.Client.Services;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
var isHostedByApi = string.IsNullOrEmpty(apiBaseUrl) ||
    builder.HostEnvironment.BaseAddress.TrimEnd('/').Equals(apiBaseUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);

if (!isHostedByApi)
{
    builder.RootComponents.Add<Routes>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");
}

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddScoped(sp =>
    (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddTransient<BearerTokenHandler>();

builder.Services.AddScoped<TimeZoneService>();

var apiBaseAddress = new Uri(
    builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = apiBaseAddress,
});

builder
    .Services.AddHttpClient(
        "MechanicShopClient",
        client => client.BaseAddress = apiBaseAddress)
    .AddHttpMessageHandler<BearerTokenHandler>();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<ServiceApi>();

await builder.Build().RunAsync();
