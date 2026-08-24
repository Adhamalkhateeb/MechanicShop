using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.Extensions.Caching.Hybrid;

using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;

public sealed class RemoveCustomerCommandHandler(
    ILogger<RemoveCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache) : IRequestHandler<RemoveCustomerCommand, Result<Deleted>>
{
    private readonly ILogger<RemoveCustomerCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Deleted>> Handle(RemoveCustomerCommand command, CancellationToken ct)
    {
        var customer = await _context.Customers.FindAsync([command.CustomerId], ct);

        if (customer is null)
        {
            _logger.LogWarning("Trying to remove a non-existent customer: {CustomerId}", command.CustomerId);
            return ApplicationErrors.CustomerNotFound;
        }

        // TODO: Check if there is active work orders for this customer.

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(ct);
        await _cache.RemoveByTagAsync("customer", ct);

        _logger.LogInformation("Customer {CustomerId} deleted successfully.", command.CustomerId);

        return Result.Deleted;
    }
}