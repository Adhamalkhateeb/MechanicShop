using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.Features.RepairTasks.Dtos;

public sealed record RepairTaskDto(
    Guid RepairTaskId,
    string Name,
    decimal LaborCost,
    decimal TotalCost,
    RepairDurationInMinutes EstimatedDurationInMinutes,
    IEnumerable<PartDto> Parts
);