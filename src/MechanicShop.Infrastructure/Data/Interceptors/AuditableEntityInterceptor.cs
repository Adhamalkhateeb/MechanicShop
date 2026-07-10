using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MechanicShop.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor(IUser user, TimeProvider dateTime) : SaveChangesInterceptor
{
    private readonly IUser _user;
    private readonly TimeProvider _dateTime;

    public  InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (
                entry.State is EntityState.Added or EntityState.Modified
                || entry.HasChangedOwnedEntities()
            )
            {
                var utcNow = _dateTime.GetUtcNow();

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = _user.Id;
                    entry.Entity.CreatedAtUtc = utcNow;
                }

                entry.Entity.LastModifiedBy = _user.Id;
                entry.Entity.LastModifiedAtUtc = utcNow;

                foreach (var ownedEntity in entry.References)
                {
                    if (
                        ownedEntity.TargetEntry is { Entity: AuditableEntity ownedAuditableEntity }
                        && ownedEntity.TargetEntry.State
                            is EntityState.Added
                                or EntityState.Modified
                    )
                    {
                        if (ownedEntity.TargetEntry.State == EntityState.Added)
                        {
                            ownedAuditableEntity.CreatedBy = _user.Id;
                            ownedAuditableEntity.CreatedAtUtc = utcNow;
                        }

                        ownedAuditableEntity.LastModifiedBy = _user.Id;
                        ownedAuditableEntity.LastModifiedAtUtc = utcNow;
                    }
                }
            }
        }
    }
}

public static class EntityEntryExtensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry)
    {
        return entry.References.Any(r =>
            r.TargetEntry?.Metadata.IsOwned() == true
            && (
                r.TargetEntry.State == EntityState.Added
                || r.TargetEntry.State == EntityState.Modified
            )
        );
    }
}
