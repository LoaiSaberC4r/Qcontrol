using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class ServiceGlobalizationRequestItemConfiguration
    : IEntityTypeConfiguration<ServiceGlobalizationRequestItem>,
      IWriteEntityConfiguration
{
    public void Configure(
        EntityTypeBuilder<ServiceGlobalizationRequestItem> builder)
    {
        builder.ToTable("ServiceGlobalizationRequestItem");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.RequestId)
            .IsRequired();

        builder.Property(x => x.ServiceId)
            .IsRequired();

        builder.HasOne(x => x.Request)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Service)
            .WithMany()
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RequestId)
            .HasDatabaseName(
                "IX_ServiceGlobalizationRequestItem_RequestId");

        builder.HasIndex(x => x.ServiceId)
            .HasDatabaseName(
                "IX_ServiceGlobalizationRequestItem_ServiceId");

        builder.HasIndex(x => new
            {
                x.RequestId,
                x.ServiceId
            })
            .IsUnique()
            .HasDatabaseName(
                "UX_ServiceGlobalizationRequestItem_RequestId_ServiceId");
    }
}
