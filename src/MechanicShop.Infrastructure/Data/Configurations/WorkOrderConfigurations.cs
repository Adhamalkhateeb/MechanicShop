

using MechanicShop.Domain.Workorders;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class WorkOrderConfigurations : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.HasKey(w => w.Id).IsClustered(false);

        builder.Property(w => w.VehicleId).IsRequired();

        builder.Property(w => w.LaborId).IsRequired();

        builder.Property(w => w.StartAtUtc).IsRequired();
        builder.Property(w => w.EndAtUtc).IsRequired();

        builder.Property(w => w.Spot).HasConversion<string>().IsRequired();
        builder.Property(w => w.State).HasConversion<string>().IsRequired();

        builder.HasMany(w => w.RepairTasks)
            .WithMany()
            .UsingEntity(j => j.ToTable("WorkOrderRepairTasks"));

        builder.HasOne(w => w.Labor)
            .WithMany()
            .HasForeignKey(w => w.LaborId);

        builder.HasOne(w => w.Vehicle)
            .WithMany()
            .HasForeignKey(w => w.VehicleId);

        builder.HasIndex(w => w.VehicleId);
        builder.HasIndex(w => w.LaborId);
        builder.HasIndex(w => w.State);
        builder.HasIndex(w => new { w.StartAtUtc, w.EndAtUtc });
    }
}
