using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Customers;

namespace MechanicShop.Application.Features.Customers.Mappers;

public static class CustomerMapper
{
    public static CustomerDto ToDto(this Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        return new CustomerDto
        {
            CustomerId = customer.Id,
            Name = customer.Name,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Vehicles = customer.Vehicles?.ToDtos() ?? []
        };
    }

    public static List<CustomerDto> ToDtos(this IEnumerable<Customer> customers)
    {
        return [.. customers.Select(x => x.ToDto())];
    }
}
