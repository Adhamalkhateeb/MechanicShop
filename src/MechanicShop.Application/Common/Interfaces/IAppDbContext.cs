using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.Workorders;

using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<RefreshToken> RefreshTokens { get; }
    public DbSet<Customer> Customers { get; }
    public DbSet<Vehicle> Vehicles { get; }

    public DbSet<RepairTask> RepairTasks { get; }
    public DbSet<Part> Parts { get; }

    public DbSet<Employee> Employees { get; }

    public DbSet<WorkOrder> WorkOrders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
