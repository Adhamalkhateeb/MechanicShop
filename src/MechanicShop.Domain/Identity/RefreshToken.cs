using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Identity;

public sealed class RefreshToken : AuditableEntity
{
    public string? Token { get; }
    public string? UserId { get; }
    public DateTimeOffset ExpiresAtUtc { get; }

    private RefreshToken() { }

    private RefreshToken(Guid id, string? userId, string? token, DateTimeOffset expiresAtUtc)
        : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static Result<RefreshToken> Create(
        Guid id,
        string? userId,
        string? token,
        DateTimeOffset expiresAtUtc)
    {
        if (id == Guid.Empty)
        {
            return RefreshTokenErrors.IdRequired;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return RefreshTokenErrors.TokenRequired;
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return RefreshTokenErrors.UserIdRequired;
        }

        if (expiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return RefreshTokenErrors.ExpiryInvalid;
        }

        return new RefreshToken(id, userId, token, expiresAtUtc);
    }
}
