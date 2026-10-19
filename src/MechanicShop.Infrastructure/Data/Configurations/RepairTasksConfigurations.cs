

using MechanicShop.Domain.RepairTasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

internal class RepairTasksConfigurations : IEntityTypeConfiguration<RepairTask>
{
    public void Configure(EntityTypeBuilder<RepairTask> builder)
    {

        builder.HasKey(rt => rt.Id).IsClustered(false);
        builder.Property(rt => rt.Id).ValueGeneratedNever();

        builder.Property(rt => rt.Name).HasMaxLength(100).IsRequired();

        builder.Property(rt => rt.LaborCost).HasColumnType("decimal(18, 2)").IsRequired();

        builder.Ignore(rt => rt.TotalCost);

        builder.Property(rt => rt.EstimatedDurationInMins).HasConversion<string>().IsRequired();

        builder.HasMany(rt => rt.Parts)
            .WithOne()
            .HasForeignKey("RepairTaskId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(rt => rt.Parts).UsePropertyAccessMode(PropertyAccessMode.Field);

    }
}

