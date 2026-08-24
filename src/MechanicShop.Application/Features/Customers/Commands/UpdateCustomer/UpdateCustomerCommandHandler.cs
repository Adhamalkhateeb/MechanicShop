using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler
    (IAppDbContext context, ILogger<UpdateCustomerCommandHandler> logger, HybridCache cache) : IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger = logger;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(UpdateCustomerCommand command, CancellationToken ct)
    {
        var customer = await _context.Customers
            .Include(rt => rt.Vehicles)
            .FirstOrDefaultAsync(rt => rt.Id == command.CustomerId, ct);

        if (customer is null)
        {
            _logger.LogWarning("Customer {CustomerId} not found for update.", command.CustomerId);

            return ApplicationErrors.CustomerNotFound;
        }

        var email = command.Email.Trim().ToLower();
        var phone = command.PhoneNumber.Trim();

        var emailExists = await _context.Customers.AnyAsync(c => c.Email == email && c.Id != command.CustomerId);
        if (emailExists)
        {
            _logger.LogWarning("Customer update aborted due to email conflict.");
            return CustomerErrors.CustomerEmailExists;
        }

        var phoneExists = await _context.Customers.AnyAsync(c => c.PhoneNumber == phone && c.Id != command.CustomerId);
        if (phoneExists)
        {
            _logger.LogWarning("Customer update aborted due to phone number conflict.");
            return CustomerErrors.CustomerPhoneExists;
        }

        List<Vehicle> vehicles = [];

        foreach (var v in command.Vehicles)
        {
            var vehicleId = v.VehicleId ?? Guid.NewGuid();

            var createVehicleResult = Vehicle.Create(vehicleId, v.Make, v.Model, v.Year, v.LicensePlate);
            if (createVehicleResult.IsFailure)
            {
                return createVehicleResult.Errors;
            }

            vehicles.Add(createVehicleResult.Value);
        }

        var updateCustomerResult = customer.Update(command.Name, email, phone);

        if (updateCustomerResult.IsFailure)
        {
            return updateCustomerResult.Errors;
        }

        var upsertVehiclesResult = customer.UpsertVehicles(vehicles);

        if (upsertVehiclesResult.IsFailure)
        {
            return upsertVehiclesResult.Errors;
        }

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("customer");

        return Result.Updated;
    }
}

