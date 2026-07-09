using System.Security.Claims;
using MechanicShop.Application.Common.Interfaces;

namespace MechanicShop.Api.Infrastructure.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _contextAccessor;

    public CurrentUser(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public string? Id =>
        _contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
