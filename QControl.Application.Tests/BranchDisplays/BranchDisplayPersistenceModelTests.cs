using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.Application.Tests.BranchDisplays;

public sealed class BranchDisplayPersistenceModelTests
{
    [Fact]
    public void Model_enforces_single_configuration_and_runtime_message_ordering()
    {
        var options = new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=QControlModelTests;Trusted_Connection=True;")
            .Options;
        using var context = new PlatformWriteDbContext(
            options,
            new TestTenantContext(),
            new TestCurrentBranchContext());
        var model = context.GetService<IDesignTimeModel>().Model;

        var configuration = model.FindEntityType(typeof(BranchDisplayConfiguration))!;
        Assert.True(configuration.GetIndexes().Single(index =>
            index.Properties.Single().Name == nameof(BranchDisplayConfiguration.BranchId)).IsUnique);
        Assert.Equal(DeleteBehavior.Restrict, configuration.GetForeignKeys().Single(key =>
            key.Properties.Single().Name == nameof(BranchDisplayConfiguration.BranchId)).DeleteBehavior);
        Assert.True(configuration.FindProperty(nameof(BranchDisplayConfiguration.RowVersion))!.IsConcurrencyToken);
        Assert.Equal(7, configuration.FindProperty(nameof(BranchDisplayConfiguration.DisplayBackgroundColor))!.GetMaxLength());
        Assert.False(configuration.FindProperty(nameof(BranchDisplayConfiguration.DisplayBackgroundColor))!.IsUnicode());

        var message = model.FindEntityType(typeof(BranchDisplayMessage))!;
        Assert.True(message.GetIndexes().Single(index =>
            index.Properties.Select(x => x.Name).SequenceEqual(new[]
            {
                nameof(BranchDisplayMessage.BranchId),
                nameof(BranchDisplayMessage.DisplayOrder)
            })).IsUnique);
        Assert.Contains(message.GetIndexes(), index =>
            index.Properties.Select(x => x.Name).SequenceEqual(new[]
            {
                nameof(BranchDisplayMessage.BranchId),
                nameof(BranchDisplayMessage.IsActive),
                nameof(BranchDisplayMessage.DisplayOrder)
            }));
        Assert.Equal(DeleteBehavior.Restrict, message.GetForeignKeys().Single(key =>
            key.Properties.Single().Name == nameof(BranchDisplayMessage.BranchId)).DeleteBehavior);
        Assert.True(message.FindProperty(nameof(BranchDisplayMessage.RowVersion))!.IsConcurrencyToken);
        Assert.Equal(500, message.FindProperty(nameof(BranchDisplayMessage.TextAr))!.GetMaxLength());
    }
}
