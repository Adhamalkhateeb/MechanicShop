using MechanicShop.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Infrastructure.Data;

public class ApplicationDbContextInitialiser(
    ILogger<ApplicationDbContextInitialiser> logger,
    AppDbContext context,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager
)
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger = logger;
    private readonly AppDbContext _context = context;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Seed default roles
        var roles = new[] { "Manager", "Labor" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
                _logger.LogInformation("Created role: {Role}", role);
            }
        }

        // Seed default admin user
        var adminEmail = "admin@mechanicshop.com";
        if (await _userManager.FindByEmailAsync(adminEmail) is null)
        {
            var adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin@123!");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Manager");
                _logger.LogInformation("Created default admin user: {Email}", adminEmail);
            }
            else
            {
                _logger.LogWarning("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
