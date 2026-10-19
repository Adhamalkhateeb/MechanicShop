using MechanicShop.Domain.RepairTasks.Parts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class PartConfigurations : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.HasKey(p => p.Id).IsClustered(false);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Cost).HasColumnType("decimal(18, 2)").IsRequired();
        builder.Property(p => p.Quantity).IsRequired();
    }
}

