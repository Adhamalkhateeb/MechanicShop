using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCutomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCutomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email address")
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?\d{7,15}$").WithMessage("Phone number must be between 7 and 15 digits and may start with '+'.");

        RuleFor(x => x.Vehicles)
            .NotNull().WithMessage("Vehicles can't be null")
            .Must(v => v.Count > 0).WithMessage("At least one vehicle is required");

        RuleForEach(c => c.Vehicles)
            .SetValidator(new CreateVehicleCommandValidator());
    }
}
