using System.Security.Claims;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MechanicShop.Api.Services;

public class CurrentUser(IHttpContextAccessor contextAccessor) : IUser
{
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;

    public string? Id =>
        _contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
