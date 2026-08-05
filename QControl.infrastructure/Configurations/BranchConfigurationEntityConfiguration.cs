using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class BranchConfigurationEntityConfiguration
    : IEntityTypeConfiguration<BranchConfiguration>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<BranchConfiguration> builder)
    {
        builder.ToTable("BranchConfiguration");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.AllowedTime)
            .IsRequired()
            .HasColumnType("time(0)");

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(x => x.BranchId)
            .IsUnique()
            .HasDatabaseName("UX_BranchConfiguration_BranchId");

        builder.HasOne(x => x.Branch)
            .WithOne(x => x.Configuration)
            .HasForeignKey<BranchConfiguration>(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
