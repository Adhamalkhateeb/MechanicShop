using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateTokens;

public sealed class GenerateTokenQueryValidator : AbstractValidator<GenerateTokenQuery>
{
    public GenerateTokenQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode("Email:Required")
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithErrorCode("Email:InvalidFormat")
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode("Password:Required")
            .WithMessage("Password is required.");
    }
}
