using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class ServiceConfiguration
    : IEntityTypeConfiguration<Service>,
      IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Service", table =>
        {
            table.HasCheckConstraint(
                "CK_Service_ArabicName_NotBlank",
                "LEN(LTRIM(RTRIM([ArabicName]))) > 0");

            table.HasCheckConstraint(
                "CK_Service_EnglishName_NotBlank",
                "LEN(LTRIM(RTRIM([EnglishName]))) > 0");

            table.HasCheckConstraint(
                "CK_Service_ServiceCode_Requirement",
                "([IsServiceCodeRequired] = 1 AND [ServiceCode] IS NOT NULL AND LEN(LTRIM(RTRIM([ServiceCode]))) > 0) OR ([IsServiceCodeRequired] = 0 AND [ServiceCode] IS NULL)");

            table.HasCheckConstraint(
                "CK_Service_RangePrefix_NullOrNotBlank",
                "[RangePrefix] IS NULL OR LEN(LTRIM(RTRIM([RangePrefix]))) > 0");

            table.HasCheckConstraint(
                "CK_Service_RangeStart_NullOrNonNegative",
                "[RangeStartNumber] IS NULL OR [RangeStartNumber] >= 0");

            table.HasCheckConstraint(
                "CK_Service_RangeEnd_NullOrNonNegative",
                "[RangeEndNumber] IS NULL OR [RangeEndNumber] >= 0");

            table.HasCheckConstraint(
                "CK_Service_RangeEnd_NullOrGreaterOrEqualStart",
                "[RangeStartNumber] IS NULL OR [RangeEndNumber] IS NULL OR [RangeEndNumber] >= [RangeStartNumber]");

            table.HasCheckConstraint(
                "CK_Service_NoOfTicketCopies_NullOrPositive",
                "[NoOfTicketCopies] IS NULL OR [NoOfTicketCopies] > 0");

            table.HasCheckConstraint(
                "CK_Service_WaitingDuration_NullOrNonNegative",
                "[WaitingDuration] IS NULL OR [WaitingDuration] >= 0");

            table.HasCheckConstraint(
                "CK_Service_TicketIssuable_Settings_Required",
                "[IsTicketIssuable] = 0 OR ([RangePrefix] IS NOT NULL AND LEN(LTRIM(RTRIM([RangePrefix]))) > 0 AND [RangeStartNumber] IS NOT NULL AND [RangeEndNumber] IS NOT NULL AND [WaitingDuration] IS NOT NULL AND [NoOfTicketCopies] IS NOT NULL)");

            table.HasCheckConstraint(
                "CK_Service_OrderNo_NonNegative",
                "[OrderNo] >= 0");

            table.HasCheckConstraint(
                "CK_Service_Priority_NonNegative",
                "[Priority] >= 0");

            table.HasCheckConstraint(
                "CK_Service_Scope_Owner",
                "([Scope] = 1 AND [OwnerBranchId] IS NULL) OR ([Scope] = 2 AND [OwnerBranchId] IS NOT NULL)");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ParentServiceId)
            .IsRequired(false);

        builder.Property(x => x.Scope)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.OwnerBranchId)
            .IsRequired(false);

        builder.Property(x => x.ArabicName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EnglishName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ServiceCode)
            .HasMaxLength(ServiceCodeNormalizer.MaxLength)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.IsServiceCodeRequired)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.ArabicUserMessage)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.EnglishUserMessage)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.IsTicketIssuable)
            .IsRequired();

        builder.Property(x => x.IsClientInputRequired)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.HasReservation)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.OrderNo)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.RangePrefix)
            .HasMaxLength(10)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.RangeStartNumber)
            .IsRequired(false);

        builder.Property(x => x.RangeEndNumber)
            .IsRequired(false);

        builder.Property(x => x.WaitingDuration)
            .IsRequired(false);

        builder.Property(x => x.NoOfTicketCopies)
            .IsRequired(false);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Property(x => x.CreatedByApplicationUserId)
            .IsRequired();

        builder.Property(x => x.LastModifiedByApplicationUserId)
            .IsRequired(false);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.DeletedOnUtc)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.Property(x => x.RestoredOnUtc)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.HasOne(x => x.ParentService)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OwnerBranch)
            .WithMany(x => x.OwnedServices)
            .HasForeignKey(x => x.OwnerBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LastModifiedByApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.LastModifiedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ParentServiceId);

        builder.HasIndex(x => x.Scope)
            .HasDatabaseName("IX_Service_Scope");

        builder.HasIndex(x => x.OwnerBranchId)
            .HasDatabaseName("IX_Service_OwnerBranchId");

        builder.HasIndex(x => new
        {
            x.Scope,
            x.OwnerBranchId
        })
        .HasDatabaseName("IX_Service_Scope_OwnerBranchId");

        builder.HasIndex(x => new
        {
            x.ParentServiceId,
            x.Scope,
            x.OwnerBranchId
        })
        .HasDatabaseName("IX_Service_ParentServiceId_Scope_OwnerBranchId");

        builder.HasIndex(x => x.IsActive);

        builder.HasIndex(x => x.IsDeleted);

        builder.HasIndex(x => x.ServiceCode)
            .IsUnique()
            .HasFilter("[ServiceCode] IS NOT NULL")
            .HasDatabaseName("UX_Service_ServiceCode");

        builder.HasIndex(x => x.CreatedByApplicationUserId);

        builder.HasIndex(x => x.LastModifiedByApplicationUserId);

        builder.HasIndex(x => new
        {
            x.ParentServiceId,
            x.OrderNo
        })
        .HasDatabaseName("IX_Service_ParentServiceId_OrderNo");

        builder.HasIndex(x => new
        {
            x.ArabicName,
            x.ParentServiceId,
            x.Scope,
            x.OwnerBranchId
        })
        .IsUnique()
        .HasFilter("[IsDeleted] = 0")
        .HasDatabaseName("UX_Service_ArabicName_ParentServiceId_Scope_OwnerBranchId");

        builder.HasIndex(x => new
        {
            x.EnglishName,
            x.ParentServiceId,
            x.Scope,
            x.OwnerBranchId
        })
        .IsUnique()
        .HasFilter("[IsDeleted] = 0")
        .HasDatabaseName("UX_Service_EnglishName_ParentServiceId_Scope_OwnerBranchId");
    }
}
