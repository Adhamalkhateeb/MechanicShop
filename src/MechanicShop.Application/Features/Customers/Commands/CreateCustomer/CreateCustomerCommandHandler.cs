using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler
    (IAppDbContext context, ILogger<CreateCustomerCommandHandler> logger, HybridCache cache) : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly HybridCache _cache = cache;
    private readonly IAppDbContext _context = context;
    private readonly ILogger<CreateCustomerCommandHandler> _logger = logger;
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand command, CancellationToken ct)
    {
        var email = command.Email.Trim().ToLower();
        var phoneNumber = command.PhoneNumber.Trim();

        var emailExists = await _context.Customers.AnyAsync(c => c.Email.ToLower() == email, ct);

        if (emailExists)
        {
            _logger.LogWarning("Customer creation aborted. Email already exists.");
            return CustomerErrors.CustomerEmailExists;
        }

        var phoneNumberExists = await _context.Customers.AnyAsync(c => c.PhoneNumber == phoneNumber, ct);

        if (phoneNumberExists)
        {
            _logger.LogWarning("Customer creation aborted. Phone number already exists.");
            return CustomerErrors.CustomerPhoneExists;
        }

        List<Vehicle> vehicles = [];

        foreach (var v in command.Vehicles)
        {
            var createVehicleResult = Vehicle.Create(Guid.NewGuid(), v.Make, v.Model, v.Year, v.LicensePlate);

            if (createVehicleResult.IsFailure)
            {
                return createVehicleResult.Errors;
            }

            vehicles.Add(createVehicleResult.Value);
        }

        var createCustomerResult = Customer.Create(Guid.NewGuid(), command.Name.Trim(), email, phoneNumber, vehicles);

        if (createCustomerResult.IsFailure)
        {
            return createCustomerResult.Errors;
        }

        _context.Customers.Add(createCustomerResult.Value);

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("customer", ct);

        var customer = createCustomerResult.Value;

        _logger.LogInformation("Customer created successfully. Id: {CustomerId}", createCustomerResult.Value.Id);

        return customer.ToDto();
    }
}