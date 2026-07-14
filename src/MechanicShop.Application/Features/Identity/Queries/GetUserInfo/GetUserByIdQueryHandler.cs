using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfo;

public sealed class GetUserByIdQueryHandler(IIdentityService identityService, ILogger<GetUserByIdQueryHandler> logger) : IRequestHandler<GetUserByIdQuery, Result<AppUserDto>>
{
    private readonly IIdentityService _identityService = identityService;
    private readonly ILogger<GetUserByIdQueryHandler> _logger = logger;

    public async Task<Result<AppUserDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var getUserResult = await _identityService.GetUserByIdAsync(query.UserId!);

        if (getUserResult.IsFailure)
        {
            _logger.LogError("Failed to retrieve user with id {UserId} {ErrorDetails}", query.UserId, getUserResult.TopError.Description);
            return getUserResult.Errors;
        }

        return getUserResult.Value;
    }
}
