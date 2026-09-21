using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks;

public static class RepairTaskErrors
{
    public static Error NameRequired => Error.Validation("RepairTask:Name:Required", "Name is required.");

    public static Error LaborCostInvalid => Error.Validation("RepairTask:LaborCost:Invalid", "Labor cost must be between 10,000.");

    public static Error PartsRequired => Error.Validation("RepairTask:Parts:Required", "Parts are required.");

    public static Error EstimatedDurationInvalid => Error.Validation("RepairTask:EstimatedDuration:Invalid", "Invalid duration selected.");
}

