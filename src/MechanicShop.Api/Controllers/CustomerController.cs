using Asp.Versioning;

using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Application.Features.Customers.Queries.GetCutsomers;
using MechanicShop.Contracts.Requests.Customers;
using MechanicShop.Domain.Identity;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/customers")]
[ApiVersion("1.0")]
[Authorize]
public sealed class CustomerController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType<List<CustomerDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a list of customers.")]
    [EndpointDescription("Returns all customers associated with the current user.")]
    [EndpointName("GetCustomers")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [OutputCache(Duration = 60)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetCustomersQuery(), ct);

        return result.Match(Ok, Problem);
    }

    [HttpGet("{customerId:guid}", Name = "GetCustomerById")]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a specific customer by Id.")]
    [EndpointDescription("Returns a details of a specific customer if found.")]
    [EndpointName("GetCustomerById")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60)]
    public async Task<IActionResult> Get(Guid customerId, CancellationToken ct)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(customerId), ct);

        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a new customer.")]
    [EndpointDescription("Adds a new customer to the system.")]
    [EndpointName("CreateCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var vehicles = request.Vehicles
            .ConvertAll(v => new CreateVehicleCommand(v.Make, v.Model, v.Year, v.LicensePlate));

        var result = await sender.Send(new CreateCustomerCommand(request.Name, request.PhoneNumber, request.Email, vehicles), ct);

        return result.Match(
            response => CreatedAtRoute(
                routeName: "GetCustomerById",
                routeValues: new { version = "1.0", customerId = response.CustomerId },
                value: response),
            Problem);
    }

    [HttpPut("{customerId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Updates an existing customer.")]
    [EndpointDescription("Updates a customer and its associated vehicle.")]
    [EndpointName("UpdateCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update(Guid customerId, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var vehicles = request.Vehicles
            .ConvertAll(v => new UpdateVehicleCommand(v.VehicleId, v.Make, v.Model, v.Year, v.LicensePlate));

        var result = await sender.Send(new UpdateCustomerCommand(customerId, request.Name, request.PhoneNumber, request.Email, vehicles), ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpDelete("{customerId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Removes a customer.")]
    [EndpointDescription("Deletes the specified customer from the system.")]
    [EndpointName("RemoveCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid customerId, CancellationToken ct)
    {
        var result = await sender.Send(new RemoveCustomerCommand(customerId), ct);

        return result.Match(_ => NoContent(), Problem);
    }
}

