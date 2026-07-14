using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Utilities;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

using Microsoft.AspNetCore.Identity;

namespace MechanicShop.Infrastructure.Identity;

public sealed class IdentityService(UserManager<AppUser> userManager) : IIdentityService
{
    private readonly UserManager<AppUser> _userManager = userManager;

    public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Error.NotFound(
                "Identity:UserNotFound",
                $"User with email '{UtilityService.MaskEmail(email)}' not found.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Error.Unauthorized(
                "Identity:AccountLockedOut",
                "Account is temporarily locked due to too many failed login attempts. Please try again later.");
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            await _userManager.AccessFailedAsync(user);

            return Error.Unauthorized("Identity:InvalidCredentials", "Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        return new AppUserDto(
            user.Id,
            user.Email!,
            await _userManager.GetRolesAsync(user),
            await _userManager.GetClaimsAsync(user));
    }

    public async Task<Result<AppUserDto>> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Error.NotFound("Identity:UserNotFound", $"User with ID '{userId}' not found.");
        }

        return new AppUserDto(
            user.Id,
            user.Email!,
            await _userManager.GetRolesAsync(user),
            await _userManager.GetClaimsAsync(user));
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.UserName;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is not null && await _userManager.IsInRoleAsync(user, role);
    }
}
