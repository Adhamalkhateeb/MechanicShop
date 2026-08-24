using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Identity;

using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<RefreshToken> RefreshTokens { get; }
    public DbSet<Customer> Customers { get; }
    public DbSet<Vehicle> Vehicles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
