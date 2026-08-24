using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Customers.Vehicles;

namespace MechanicShop.Application.Features.Customers.Mappers;

public static class VehicleMapper
{
    public static VehicleDto ToDto(this Vehicle entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new VehicleDto(entity.Id, entity.Make!, entity.Model!, entity.Year, entity.LicensePlate!);
    }

    public static List<VehicleDto> ToDtos(this IEnumerable<Vehicle> vehicles)
    {
        return [.. vehicles.Select(v => v.ToDto())];
    }
}
