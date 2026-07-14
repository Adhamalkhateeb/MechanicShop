namespace MechanicShop.Application.Features.Identity;

public sealed record TokenResponse(
    string? AccessToken,
    string? RefreshToken,
    DateTimeOffset ExpiresAtUtc
);
