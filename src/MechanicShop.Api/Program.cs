using MechanicShop.Api;
using MechanicShop.Api.Components;
using MechanicShop.Api.Extensions;
using MechanicShop.Application;
using MechanicShop.Infrastructure;

using Scalar.AspNetCore;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services
    .AddPresentation(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddApplication();

builder.Host.UseSerilog(
    (context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "MechanicShop API V1");

        options.EnableDeepLinking();
        options.DisplayRequestDuration();
        options.EnableFilter();
    });

    app.MapScalarApiReference();

    await app.InitialiseDatabaseAsync();

    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}

app.UseCoreMiddlewares();

app.MapControllers();

app.UseAntiforgery();

app.UseStaticFiles();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AllowAnonymous()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MechanicShop.Client._Imports).Assembly);

app.Run();
