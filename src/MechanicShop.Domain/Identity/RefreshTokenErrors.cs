using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Identity;

public static class RefreshTokenErrors
{
    public static Error IdRequired =>
        Error.Validation(
            code: "RefreshToken:Id:Required",
            description: "Refresh token ID is required.");
    public static Error TokenRequired =>
        Error.Validation(
            code: "RefreshToken:Token:Required",
            description: "Token value is required.");

    public static Error UserIdRequired =>
        Error.Validation(code: "RefreshToken:UserId:Required", description: "User ID is required.");

    public static Error ExpiryInvalid =>
        Error.Validation(
            code: "RefreshToken:Expiry:Invalid",
            description: "Refresh token expiry date must be in the future.");
}
