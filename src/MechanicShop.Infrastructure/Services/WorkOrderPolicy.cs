using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Workorders.Enums;
using MechanicShop.Infrastructure.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MechanicShop.Infrastructure.Services;

public class WorkOrderPolicy(IOptions<AppSettings> options, IAppDbContext context) : IWorkOrderPolicy
{
    private readonly AppSettings _settings = options.Value;
    private readonly IAppDbContext _context = context;
    public async Task<Result<Success>> CheckSpotAvailability(Spot spot, DateTimeOffset startTime, DateTimeOffset endTime, Guid? excludedWorkOrderId = null, CancellationToken ct = default)
    {
        var isOccupied = await _context.WorkOrders.AnyAsync(
            a =>
             a.Spot == spot &&
             a.StartAtUtc < endTime && a.EndAtUtc > startTime &&
             (!excludedWorkOrderId.HasValue || a.Id != excludedWorkOrderId.Value),
            ct);

        return isOccupied
             ? Error.Conflict("MechanicShop:Spot:Full", "The selected spot is full.")
             : Result.Success;
    }

    public async Task<bool> IsLaborOccupiedAsync(Guid laborId, Guid execludedWorkOrderId, DateTimeOffset startTime, DateTimeOffset endTime, CancellationToken ct = default)
    {
        return await _context.WorkOrders
                .AnyAsync(w =>
                    w.LaborId == laborId &&
                    (execludedWorkOrderId == Guid.Empty || w.Id != execludedWorkOrderId) &&
                    w.StartAtUtc < endTime && w.EndAtUtc > startTime);
    }

    public bool IsOutSideOperatingHours(DateTimeOffset startTime, TimeSpan duration)
    {
        var opening = startTime.Date.Add(_settings.OpeningTime.ToTimeSpan());
        var closing = startTime.Date.Add(_settings.ClosingTime.ToTimeSpan());
        var endTime = startTime + duration;

        return endTime > closing || startTime < opening;
    }

    public async Task<bool> IsVehicleAlreadyScheduledAsync(Guid vehicleId, DateTimeOffset startTime, DateTimeOffset endTime, Guid? excludedWorkOrderId = null, CancellationToken ct = default)
    {
        return await _context.WorkOrders
             .AnyAsync(w =>
                 w.VehicleId == vehicleId &&
                 (excludedWorkOrderId == null || w.Id != excludedWorkOrderId) &&
                 w.StartAtUtc < endTime && w.EndAtUtc > startTime);

    }

    public Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if ((endAt - startAt) < TimeSpan.FromMinutes(_settings.MinimumAppointmentDurationInMinutes))
        {
            return Error.Conflict("WorkOrder:TooShortDuration", $"Minimum duration is {_settings.MinimumAppointmentDurationInMinutes} minutes");
        }

        return Result.Success;
    }
}

