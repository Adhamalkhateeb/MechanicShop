using MechanicShop.Infrastructure.Data;

namespace MechanicShop.Api.Extensions;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}
