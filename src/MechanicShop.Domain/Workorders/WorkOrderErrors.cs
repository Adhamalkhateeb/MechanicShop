

using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Workorders.Enums;

namespace MechanicShop.Domain.Workorders;

public static class WorkOrderErrors
{
    public static Error WorkOrderIdRequired => Error.Validation(
        "WorkOrder:Id:Required",
        "WorkOrder Id is required");

    public static Error VehicleIdRequired => Error.Validation(
        "WorkOrder:VehicleId:Required",
        "Vehicle Id is required");

    public static Error LaborIdRequired => Error.Validation(
            "WorkOrder:LaborId:Required",
            "Labor Id is required");

    public static Error RepairTasksRequired => Error.Validation(
        "WorkOrder:RepairTasks:Required",
        "At least one repair task is required");

    public static Error InvalidTiming => Error.Conflict(
        "WorkOrder:Timing:Invalid",
        "End time must be after start time.");

    public static Error InvalidSpot => Error.Validation(
        "WorkOrder:Spot:Invalid",
        "The provided spot is invalid");

    public static Error InvalidStateTransition(WorkOrderState current, WorkOrderState next) => Error.Conflict(
        "WorkOrder:State:InvalidTransition",
        $"Cannot transition from {current} to {next}");

    public static Error WorkOrderIsNotEditable(WorkOrderState state) => Error.Conflict(
        "WorkOrder:State:NotEditable",
        $"Cannot update work order with state {state}");

    public static Error RepairTaskAlreadyAdded => Error.Conflict(
        "WorkOrder:RepairTask:AlreadyAdded",
        "Repair task already added");
}

