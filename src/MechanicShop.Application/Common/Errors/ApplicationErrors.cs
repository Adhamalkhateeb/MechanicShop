using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Errors;

public static class ApplicationErrors
{
    public static Error ExpiredAccessTokenInvalid =>
        Error.Unauthorized("Auth:ExpiredAccessToken:Invalid", "Invalid expired access token.");

    public static Error UserIdClaimNotFound =>
        Error.Unauthorized(
            "Auth:UserIdClaim:NotFound",
            "User ID claim not found in expired access token.");

    public static Error RefreshTokenInvalid =>
        Error.Unauthorized("Auth:RefreshToken:Invalid", "Invalid or expired refresh token.");
}
