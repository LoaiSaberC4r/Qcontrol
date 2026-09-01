using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BranchConfigurationEntity = QControl.Domain.Entities.BranchConfiguration;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class BranchConfigurationEntityConfiguration
    : IEntityTypeConfiguration<BranchConfigurationEntity>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<BranchConfigurationEntity> builder)
    {
        builder.ToTable("BranchConfiguration", table =>
        {
            table.HasCheckConstraint("CK_BranchConfiguration_MaxTicketCallAttempts_Positive",
                "[MaximumTicketCallAttempts] > 0");
            table.HasCheckConstraint("CK_BranchConfiguration_NoShowTimeout_Positive",
                "[TicketNoShowAutoCancellationMinutes] > 0");
            table.HasCheckConstraint("CK_BranchConfiguration_ArchiveRetention_Positive",
                "[TicketArchiveRetentionDays] > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.AllowedTime)
            .IsRequired()
            .HasColumnType("time(0)");

        builder.Property(x => x.MaximumTicketCallAttempts)
            .HasDefaultValue(3)
            .IsRequired();

        builder.Property(x => x.TicketNoShowAutoCancellationMinutes)
            .HasDefaultValue(30)
            .IsRequired();

        builder.Property(x => x.TicketArchiveRetentionDays)
            .HasDefaultValue(30)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(x => x.BranchId)
            .IsUnique()
            .HasDatabaseName("UX_BranchConfiguration_BranchId");

        builder.HasOne(x => x.Branch)
            .WithOne(x => x.Configuration)
            .HasForeignKey<BranchConfigurationEntity>(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
