using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.Application.Tests.BranchConfigurations;

public sealed class BranchConfigurationPersistenceModelTests
{
    [Fact]
    public void Model_matches_existing_branch_configuration_schema()
    {
        using var context = CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var entity = model.FindEntityType(typeof(BranchConfiguration));

        Assert.NotNull(entity);
        Assert.Equal("BranchConfiguration", entity!.GetTableName());
        Assert.Equal(
            "time(0)",
            entity.FindProperty(nameof(BranchConfiguration.AllowedTime))!
                .GetColumnType());

        var rowVersion =
            entity.FindProperty(nameof(BranchConfiguration.RowVersion))!;
        Assert.True(rowVersion.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, rowVersion.ValueGenerated);
        Assert.Equal("rowversion", rowVersion.GetColumnType());

        var branchIndex = entity.GetIndexes().Single(index =>
            index.GetDatabaseName() ==
                "UX_BranchConfiguration_BranchId");
        Assert.True(branchIndex.IsUnique);
        Assert.Equal(
            nameof(BranchConfiguration.BranchId),
            Assert.Single(branchIndex.Properties).Name);

        var foreignKey = entity.GetForeignKeys().Single(key =>
            key.Properties.Single().Name ==
                nameof(BranchConfiguration.BranchId));
        Assert.True(foreignKey.IsUnique);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        Assert.Equal(typeof(Branch), foreignKey.PrincipalEntityType.ClrType);
        Assert.NotNull(entity.GetQueryFilter());

        var branch = model.FindEntityType(typeof(Branch))!;
        Assert.Equal(
            nameof(Branch.Configuration),
            branch.FindNavigation(nameof(Branch.Configuration))!.Name);
    }

    [Fact]
    public void Historical_migration_is_restored_once_with_original_id()
    {
        var migrations = typeof(PlatformWriteDbContext).Assembly
            .GetTypes()
            .Select(type => new
            {
                Type = type,
                Attribute = type.GetCustomAttributes(
                        typeof(MigrationAttribute),
                        inherit: false)
                    .Cast<MigrationAttribute>()
                    .SingleOrDefault()
            })
            .Where(item => item.Attribute?.Id ==
                "20260805103011_AddBranchConfiguration")
            .ToList();

        var migration = Assert.Single(migrations);
        Assert.Equal("AddBranchConfiguration", migration.Type.Name);
    }

    private static PlatformWriteDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=QControlModelTests;Trusted_Connection=True;")
            .Options;

        return new PlatformWriteDbContext(
            options,
            new TestTenantContext(),
            new TestCurrentBranchContext());
    }
}
