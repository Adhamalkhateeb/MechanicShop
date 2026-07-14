using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MechanicShop.Application.Features.Identity.Queries.RefreshTokens;

public sealed class RefreshTokenQueryHandler(
    ITokenProvider tokenProvider,
    IIdentityService identityService,
    IAppDbContext context,
    ILogger<RefreshTokenQueryHandler> logger
) : IRequestHandler<RefreshTokenQuery, Result<TokenResponse>>
{
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IIdentityService _identityService = identityService;
    private readonly IAppDbContext _context = context;
    private readonly ILogger<RefreshTokenQueryHandler> _logger = logger;

    public async Task<Result<TokenResponse>> Handle(RefreshTokenQuery request, CancellationToken ct)
    {
        var principal = _tokenProvider.GetPrincipalFromExpiredToken(request.ExpiredAccessToken);

        if (principal is null)
        {
            _logger.LogError("Invalid expired access token.");
            return ApplicationErrors.ExpiredAccessTokenInvalid;
        }

        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError("User ID claim not found in expired access token.");
            return ApplicationErrors.UserIdClaimNotFound;
        }

        var userResult = await _identityService.GetUserByIdAsync(userId);
        if (userResult.IsFailure)
        {
            _logger.LogError(
                "Failed to retrieve user by ID: {UserId}. Error: {Error}",
                userId,
                userResult.TopError.Description);

            return userResult.Errors;
        }

        var user = userResult.Value;

        var hashedToken = _tokenProvider.HashToken(request.RefreshToken);

        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.Token == hashedToken && rt.UserId == userId,
            ct);

        if (refreshToken is null || refreshToken.ExpiresAtUtc < DateTimeOffset.UtcNow)
        {
            _logger.LogError("Invalid or expired refresh token for user ID: {UserId}", userId);
            return ApplicationErrors.RefreshTokenInvalid;
        }

        var tokenResult = await _tokenProvider.GenerateJwtTokenAsync(user, ct);
        if (tokenResult.IsFailure)
        {
            _logger.LogError(
                "Failed to generate new JWT token for user ID: {UserId}. Error: {Error}",
                userId,
                tokenResult.TopError.Description);

            return tokenResult.Errors;
        }

        return tokenResult.Value;
    }
}
