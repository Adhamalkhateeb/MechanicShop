using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;

public sealed class RemoveRepairTaskCommandHandler(
    ILogger<RemoveRepairTaskCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache) : IRequestHandler<RemoveRepairTaskCommand, Result<Deleted>>
{
    private readonly ILogger<RemoveRepairTaskCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Deleted>> Handle(RemoveRepairTaskCommand command, CancellationToken ct)
    {
        var repairTask = await _context.RepairTasks.FindAsync([command.RepairTaskId], ct);

        if (repairTask is null)
        {
            _logger.LogWarning("RepairTask with id {RepairTaskId} not found", command.RepairTaskId);
            return ApplicationErrors.RepairTaskNotFound;
        }


        // TODO: Check if there is any work order associated with this repair task


        _context.RepairTasks.Remove(repairTask);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("repair-task", ct);

        _logger.LogInformation("RepairTask {RepairTaskId} deleted successfully.", command.RepairTaskId);

        return Result.Deleted;

    }
}
