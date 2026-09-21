using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Application.Features.RepairTasks.Mappers;

public static class PartMapper
{
    public static PartDto ToDto(this Part part)
    {
        ArgumentNullException.ThrowIfNull(part);

        return new PartDto(
            part.Id,
            part.Name ?? string.Empty,
            part.Cost,
            part.Quantity);
    }

    public static List<PartDto> ToDtos(this IEnumerable<Part> parts)
    {
        return [.. parts.Select(ToDto)];
    }
}
