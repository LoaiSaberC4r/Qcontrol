using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QControl.Domain.Entities;

namespace Qcontrol.infrastructure.Configurations;

internal sealed class TicketPrintConfigurationEntityConfiguration
    : IEntityTypeConfiguration<TicketPrintConfiguration>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<TicketPrintConfiguration> builder)
    {
        builder.ToTable("TicketPrintConfiguration", table =>
        {
            table.HasCheckConstraint("CK_TicketPrintConfiguration_Width_Positive",
                "[TicketWidthMm] > 0");
            table.HasCheckConstraint("CK_TicketPrintConfiguration_Height_Positive",
                "[TicketHeightMm] > 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.TicketWidthMm).HasColumnType("decimal(9,2)").IsRequired();
        builder.Property(x => x.TicketHeightMm).HasColumnType("decimal(9,2)").IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnType("datetime2(3)");

        builder.HasIndex(x => x.BranchId).IsUnique()
            .HasDatabaseName("UX_TicketPrintConfiguration_BranchId");
        builder.HasOne(x => x.Branch)
            .WithOne(x => x.TicketPrintConfiguration)
            .HasForeignKey<TicketPrintConfiguration>(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Elements)
            .WithOne(x => x.TicketPrintConfiguration)
            .HasForeignKey(x => x.TicketPrintConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Elements).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class TicketPrintElementEntityConfiguration
    : IEntityTypeConfiguration<TicketPrintElement>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<TicketPrintElement> builder)
    {
        builder.ToTable("TicketPrintElement", table =>
        {
            table.HasCheckConstraint("CK_TicketPrintElement_ElementType_Valid",
                "[ElementType] IN (1,2,3,4,5,6,7,8)");
            table.HasCheckConstraint("CK_TicketPrintElement_VisibleLayout",
                "[IsVisible] = 0 OR ([XMm] IS NOT NULL AND [YMm] IS NOT NULL AND [WidthMm] IS NOT NULL AND [HeightMm] IS NOT NULL AND [XMm] >= 0 AND [YMm] >= 0 AND [WidthMm] > 0 AND [HeightMm] > 0)");
            table.HasCheckConstraint("CK_TicketPrintElement_VisibleTypography",
                "([ElementType] = 1 AND [FontSizePt] IS NULL AND [FontWeight] IS NULL AND [TextAlign] IS NULL AND [Language] IS NULL) OR ([ElementType] <> 1 AND ([IsVisible] = 0 OR ([FontSizePt] IS NOT NULL AND [FontSizePt] > 0 AND [FontWeight] IN (1,2) AND [TextAlign] IN (1,2,3) AND [Language] IN (1,2))))");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.TicketPrintConfigurationId).IsRequired();
        builder.Property(x => x.ElementType).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsVisible).IsRequired();
        builder.Property(x => x.XMm).HasColumnType("decimal(9,2)");
        builder.Property(x => x.YMm).HasColumnType("decimal(9,2)");
        builder.Property(x => x.WidthMm).HasColumnType("decimal(9,2)");
        builder.Property(x => x.HeightMm).HasColumnType("decimal(9,2)");
        builder.Property(x => x.FontSizePt).HasColumnType("decimal(6,2)");
        builder.Property(x => x.FontWeight).HasConversion<int?>();
        builder.Property(x => x.TextAlign).HasConversion<int?>();
        builder.Property(x => x.Language).HasConversion<int?>();
        builder.Property(x => x.CreatedOnUtc).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnType("datetime2(3)");

        builder.HasIndex(x => new { x.TicketPrintConfigurationId, x.ElementType })
            .IsUnique()
            .HasDatabaseName("UX_TicketPrintElement_Configuration_ElementType");
    }
}
