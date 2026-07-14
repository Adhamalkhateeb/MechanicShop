using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Utilities;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateTokens;

public class GenerateTokenQueryHandler(
    ITokenProvider tokenProvider,
    IIdentityService identityService,
    ILogger<GenerateTokenQueryHandler> logger
) : IRequestHandler<GenerateTokenQuery, Result<TokenResponse>>
{
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IIdentityService _identityService = identityService;
    private readonly ILogger<GenerateTokenQueryHandler> _logger = logger;

    public async Task<Result<TokenResponse>> Handle(
        GenerateTokenQuery request,
        CancellationToken ct)
    {
        var userResult = await _identityService.AuthenticateAsync(request.Email, request.Password);
        if (userResult.IsFailure)
        {
            return userResult.Errors;
        }

        var tokenResult = await _tokenProvider.GenerateJwtTokenAsync(userResult.Value, ct);
        if (tokenResult.IsFailure)
        {
            _logger.LogError(
                "Token generation failed for email: {Email}. Error: {Error}",
                UtilityService.MaskEmail(request.Email),
                tokenResult.TopError.Description);

            return tokenResult.Errors;
        }

        return tokenResult.Value;
    }
}
