using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Common.Interfaces;

public interface IAppDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
