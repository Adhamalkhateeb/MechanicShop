
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Workorders.Enums;

namespace MechanicShop.Application.Common.Interfaces;

public interface IWorkOrderPolicy
{
    bool IsOutSideOperatingHours(DateTimeOffset startTime, TimeSpan duration);
    Task<bool> IsLaborOccupiedAsync(Guid laborId, Guid execludedWorkOrderId, DateTimeOffset startTime, DateTimeOffset endTime, CancellationToken ct = default);
    Task<bool> IsVehicleAlreadyScheduledAsync(Guid vehicleId, DateTimeOffset startTime, DateTimeOffset endTime, Guid? excludedWorkOrderId = null, CancellationToken ct = default);
    Task<Result<Success>> CheckSpotAvailability(Spot spot, DateTimeOffset startTime, DateTimeOffset endTime, Guid? excludedWorkOrderId = null, CancellationToken ct = default);
    Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt);
}

