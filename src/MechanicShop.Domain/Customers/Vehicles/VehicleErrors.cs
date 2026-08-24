using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers.Vehicles;

public static class VehicleErrors
{
    public static Error MakeRequired =>
        Error.Validation("Vehicle:Make:Required", "Vehicle make is required.");

    public static Error ModelRequired =>
        Error.Validation("Vehicle:Model:Required", "Vehicle model is required.");

    public static Error LicensePlateRequired =>
        Error.Validation("Vehicle:LicensePlate:Required", "Vehicle license plate is required.");

    public static Error YearInvalid =>
        Error.Validation("Vehicle:Year:Invalid", "Vehicle year must be 1886 and next year.");
}

