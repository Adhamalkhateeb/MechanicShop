using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.RepairTasks;

namespace MechanicShop.Application.Features.RepairTasks.Mappers;

public static class RepairTaskMapper
{
    public static RepairTaskDto ToDto(this RepairTask repairTask)
    {
        ArgumentNullException.ThrowIfNull(repairTask);

        return new RepairTaskDto(
            repairTask.Id,
            repairTask.Name ?? string.Empty,
            repairTask.LaborCost,
            repairTask.TotalCost,
            repairTask.EstimatedDurationInMins,
            repairTask.Parts.ToDtos());
    }

    public static List<RepairTaskDto> ToDtos(this IEnumerable<RepairTask> repairTasks)
    {
        return [.. repairTasks.Select(rt => rt.ToDto())];
    }
}