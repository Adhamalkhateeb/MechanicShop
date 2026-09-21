namespace MechanicShop.Application.Features.RepairTasks.Dtos;

public sealed record PartDto(Guid PartId, string Name, decimal Cost, int Quantity);
