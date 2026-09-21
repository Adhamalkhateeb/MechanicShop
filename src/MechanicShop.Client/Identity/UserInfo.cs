namespace MechanicShop.Client.Identity;

public sealed record UserInfo(
    string UserId,
    string Email,
    IList<string>? Roles = null,
    IList<ClaimDto>? Claims = null);

public sealed record ClaimDto(string? Type, string? Value);

