using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.RefreshTokens;

public sealed class RefreshTokenQueryValidator : AbstractValidator<RefreshTokenQuery>
{
    public RefreshTokenQueryValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithErrorCode("RefreshToken:Required")
            .WithMessage("Refresh token is required.")
            .MaximumLength(500)
            .WithErrorCode("RefreshToken:TooLong")
            .WithMessage("Refresh token exceeds maximum length.");

        RuleFor(x => x.ExpiredAccessToken)
            .NotEmpty()
            .WithErrorCode("ExpiredAccessToken:Required")
            .WithMessage("Expired access token is required.");
    }
}
