using System.Security.Claims;

using Asp.Versioning;

using MechanicShop.Application.Features.Identity;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries.GenerateTokens;
using MechanicShop.Application.Features.Identity.Queries.GetUserInfo;
using MechanicShop.Application.Features.Identity.Queries.RefreshTokens;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MechanicShop.Api.Controllers
{
    [Route("identity")]
    [ApiVersionNeutral]
    public class IdentityController(ISender sender) : ApiController
    {
        [HttpPost("token/generate")]
        [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        [EndpointName("GenerateToken")]
        [EndpointSummary("Generates an access and refresh token for a valid user.")]
        [EndpointDescription("Authenticates a user using provided credentials and returns a JWT token pair.")]
        public async Task<IActionResult> GenerateToken([FromBody] GenerateTokenQuery query, CancellationToken ct)
        {
            var result = await sender.Send(query, ct);
            return result.Match(Ok, Problem);
        }

        [HttpPost("token/refresh-token")]
        [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        [EndpointName("RefreshToken")]
        [EndpointSummary("Refreshes an access token using a refresh token.")]
        [EndpointDescription("Exchanges an expired access token and a valid refresh token for a new token pair.")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenQuery query, CancellationToken ct)
        {
            var result = await sender.Send(query, ct);
            return result.Match(Ok, Problem);
        }

        [HttpGet("current-user/claims")]
        [Authorize]
        [ProducesResponseType<AppUserDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        [EndpointName("GetCurrentUserClaims")]
        [EndpointSummary("Gets the current authenticated user's info.")]
        [EndpointDescription("Returns user information for the currently authenticated user based on the access token.")]
        public async Task<IActionResult> GetCurrentUserClaims(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await sender.Send(new GetUserByIdQuery(userId), ct);

            return result.Match(Ok, Problem);
        }
    }
}
