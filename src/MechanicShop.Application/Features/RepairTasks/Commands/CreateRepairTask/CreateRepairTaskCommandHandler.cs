using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask
{
    public sealed class CreateRepairTaskCommandHandler(
        ILogger<CreateRepairTaskCommandHandler> logger,
        IAppDbContext context,
        HybridCache cache) : IRequestHandler<CreateRepairTaskCommand, Result<RepairTaskDto>>
    {
        private readonly ILogger<CreateRepairTaskCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        private readonly HybridCache _cache = cache;

        public async Task<Result<RepairTaskDto>> Handle(CreateRepairTaskCommand command, CancellationToken ct)
        {

            var nameExists = await _context.RepairTasks.AnyAsync(rt => EF.Functions.Like(rt.Name, command.Name), ct);
            if (nameExists)
            {
                _logger.LogWarning("Duplicate Reapir Task name: '{Name}'.", command.Name);

                return RepairTaskErrors.DuplicateName;
            }

            List<Part> parts = [];
            foreach (var p in command.Parts)
            {
                var partResult = Part.Create(Guid.NewGuid(), p.Name, p.Cost, p.Quantity);

                if (partResult.IsFailure)
                {
                    return partResult.Errors;
                }

                parts.Add(partResult.Value);
            }

            var repairTaskResult = RepairTask.Create(Guid.NewGuid(), command.Name!, command.LaborCost, command.EstimatedDurationInMinutes!.Value, parts);

            if (repairTaskResult.IsFailure)
            {
                return repairTaskResult.Errors;
            }

            var repairTask = repairTaskResult.Value;

            _context.RepairTasks.Add(repairTask);
            await _context.SaveChangesAsync(ct);

            await _cache.RemoveByTagAsync("repair-task", ct);

            return repairTask.ToDto();
        }
    }
}
