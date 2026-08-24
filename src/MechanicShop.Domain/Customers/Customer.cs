using System.Net.Mail;
using System.Text.RegularExpressions;

using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;

namespace MechanicShop.Domain.Customers;

public sealed class Customer : AuditableEntity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }

    private readonly List<Vehicle> _vehicles = [];
    public IEnumerable<Vehicle> Vehicles => _vehicles.AsReadOnly();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Customer() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private Customer(Guid id, string name, string email, string phoneNumber, List<Vehicle> vehicles)
        : base(id)
    {
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        _vehicles = vehicles;
    }

    public static Result<Customer> Create(Guid id, string name, string email, string phoneNumber, List<Vehicle> vehicles)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return CustomerErrors.NameRequired;
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return CustomerErrors.PhoneNumberRequired;
        }

        if (!Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
        {
            return CustomerErrors.PhoneNumberInvalid;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return CustomerErrors.EmailRequired;
        }

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            return CustomerErrors.EmailInvalid;
        }

        return new Customer(id, name, email, phoneNumber, vehicles);
    }

    public Result<Updated> Update(string name, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return CustomerErrors.NameRequired;
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return CustomerErrors.PhoneNumberRequired;
        }

        if (!Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
        {
            return CustomerErrors.PhoneNumberInvalid;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return CustomerErrors.EmailRequired;
        }

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            return CustomerErrors.EmailInvalid;
        }

        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;

        return Result.Updated;
    }

    public Result<Updated> UpsertVehicles(List<Vehicle> incomingVehicles)
    {
        _vehicles.RemoveAll(existing => incomingVehicles.All(incoming => incoming.Id != existing.Id));

        foreach (var incoming in incomingVehicles)
        {
            var existing = _vehicles.FirstOrDefault(existing => existing.Id == incoming.Id);
            if (existing is null)
            {
                _vehicles.Add(incoming);
            }
            else
            {
                var updateVehicleResult = existing.Update(incoming.Make, incoming.Model, incoming.Year, incoming.LicensePlate);

                if (updateVehicleResult.IsFailure)
                {
                    return updateVehicleResult.Errors;
                }
            }
        }

        return Result.Updated;
    }
}

