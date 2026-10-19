using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed class UpdateRepairTaskCommandHandler(
    ILogger<UpdateRepairTaskCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache) : IRequestHandler<UpdateRepairTaskCommand, Result<Updated>>
{
    private readonly ILogger<UpdateRepairTaskCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(UpdateRepairTaskCommand command, CancellationToken ct)
    {

        var repairTask = await _context.RepairTasks.Include(rt => rt.Parts)
                        .FirstOrDefaultAsync(rt => rt.Id == command.RepairTaskId, ct);

        if (repairTask is null)
        {
            _logger.LogWarning("RepairTask {RepairTaskId} not found for update.", command.RepairTaskId);
            return ApplicationErrors.RepairTaskNotFound;
        }

        var nameExits = await _context.RepairTasks.AnyAsync(rt => rt.Id != command.RepairTaskId && EF.Functions.Like(rt.Name, command.Name), ct);

        if (nameExits)
        {
            _logger.LogWarning("Duplicate Reapir Task name: '{Name}'.", command.Name);
            return RepairTaskErrors.DuplicateName;
        }

        var validatedParts = new List<Part>();

        foreach (var p in command.Parts)
        {
            var partId = p.PartId ?? Guid.NewGuid();

            var partResult = Part.Create(partId, p.Name, p.Cost, p.Quantity);

            if (partResult.IsFailure)
            {
                return partResult.Errors;
            }

            validatedParts.Add(partResult.Value);
        }

        var updateRepairTaskResult = repairTask.Update(command.Name, command.LaborCost, command.EstimatedDurationInMins);

        if (updateRepairTaskResult.IsFailure)
        {
            _logger.LogWarning("Failed to update Repair Task with Id: {RepairTaskId}", command.RepairTaskId);
            return updateRepairTaskResult.Errors;
        }

        var upsertPartsResult = repairTask.UpsertParts(validatedParts);

        if (upsertPartsResult.IsFailure)
        {
            return upsertPartsResult.Errors;
        }

        await _context.SaveChangesAsync(ct);
        await _cache.RemoveByTagAsync("repair-task", ct);

        return Result.Updated;
    }
}

