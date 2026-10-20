using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.Workorders.Enums;

namespace MechanicShop.Domain.Workorders;

public sealed class WorkOrder : AuditableEntity
{
    public Guid VehicleId { get; }
    public DateTimeOffset StartAtUtc { get; private set; }
    public DateTimeOffset EndAtUtc { get; private set; }
    public Guid LaborId { get; private set; }
    public Spot Spot { get; private set; }
    public WorkOrderState State { get; private set; }
    public Employee Labor { get; private set; }
    public Vehicle Vehicle { get; private set; }

    private readonly List<RepairTask> _repairTasks = [];
    public IEnumerable<RepairTask> RepairTasks => _repairTasks.AsReadOnly();
    public bool IsEditable => State is not (WorkOrderState.Completed or WorkOrderState.Cancelled or WorkOrderState.InProgress);

    private WorkOrder() { }

    private WorkOrder(Guid id, Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid laborId, Spot spot,
        WorkOrderState state, List<RepairTask> repairTasks)
        : base(id)
    {
        VehicleId = vehicleId;
        StartAtUtc = startAt;
        EndAtUtc = endAt;
        LaborId = laborId;
        Spot = spot;
        State = state;
        _repairTasks = repairTasks;
    }

    public static Result<WorkOrder> Create(Guid id, Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid laborId, Spot spot, List<RepairTask> repairTasks)
    {
        if (id == Guid.Empty)
        {
            return WorkOrderErrors.WorkOrderIdRequired;
        }

        if (vehicleId == Guid.Empty)
        {
            return WorkOrderErrors.VehicleIdRequired;
        }

        if (laborId == Guid.Empty)
        {
            return WorkOrderErrors.LaborIdRequired;
        }

        if (repairTasks is null || repairTasks.Count == 0)
        {
            return WorkOrderErrors.RepairTasksRequired;
        }


        if (startAt >= endAt)
        {
            return WorkOrderErrors.InvalidTiming;
        }

        if (!Enum.IsDefined(spot))
        {
            return WorkOrderErrors.InvalidSpot;
        }

        return new WorkOrder(id, vehicleId, startAt, endAt, laborId, spot, WorkOrderState.Scheduled, repairTasks);
    }

    public bool CanTranistionTo(WorkOrderState newStatus)
    {
        return (State, newStatus) switch
        {
            (WorkOrderState.Scheduled, WorkOrderState.InProgress) => true,
            (WorkOrderState.InProgress, WorkOrderState.Completed) => true,
            (_, WorkOrderState.Cancelled) when State != WorkOrderState.Completed => true,
            _ => false
        };
    }

    public Result<Updated> UpdateState(WorkOrderState newStatus)
    {
        if (!CanTranistionTo(newStatus))
        {
            return WorkOrderErrors.InvalidStateTransition(State, newStatus);
        }

        State = newStatus;
        return Result.Updated;
    }

    public Result<Updated> AddRepairTask(RepairTask repairTask)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.WorkOrderIsNotEditable(State);
        }

        if (_repairTasks.Any(t => t.Id == repairTask.Id))
        {
            return WorkOrderErrors.RepairTaskAlreadyAdded;
        }

        _repairTasks.Add(repairTask);
        return Result.Updated;
    }

    public Result<Updated> UpdateLabor(Guid laborId)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.WorkOrderIsNotEditable(State);
        }

        if (laborId == Guid.Empty)
        {
            return WorkOrderErrors.LaborIdRequired;
        }

        LaborId = laborId;
        return Result.Updated;
    }

    public Result<Updated> UpdateTiming(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.WorkOrderIsNotEditable(State);
        }

        if (startAt >= endAt)
        {
            return WorkOrderErrors.InvalidTiming;
        }

        StartAtUtc = startAt;
        EndAtUtc = endAt;
        return Result.Updated;
    }

    public Result<Updated> UpdateSpot(Spot spot)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.WorkOrderIsNotEditable(State);
        }

        if (!Enum.IsDefined(spot))
        {
            return WorkOrderErrors.InvalidSpot;
        }

        Spot = spot;
        return Result.Updated;
    }

    public Result<Updated> Cancel()
    {
        if (!CanTranistionTo(WorkOrderState.Cancelled))
        {
            return WorkOrderErrors.InvalidStateTransition(State, WorkOrderState.Cancelled);
        }

        State = WorkOrderState.Cancelled;
        return Result.Updated;
    }

    public Result<Updated> ClearRepairTasks()
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.WorkOrderIsNotEditable(State);
        }

        _repairTasks.Clear();

        return Result.Updated;
    }

}

