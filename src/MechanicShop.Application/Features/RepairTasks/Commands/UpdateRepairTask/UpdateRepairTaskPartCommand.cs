using MechanicShop.Domain.Common.Results;

using MediatR;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed record UpdateRepairTaskPartCommand(Guid? PartId, string Name, decimal Cost, int Quantity) : IRequest<Result<Success>>;


