using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.Application.Tests.BranchVideos;

public sealed class BranchVideoPersistenceModelTests
{
    [Fact]
    public void Model_has_restricted_branch_relation_rowversion_and_query_indexes()
    {
        var options = new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=QControlModelTests;Trusted_Connection=True;")
            .Options;
        using var context = new PlatformWriteDbContext(
            options,
            new TestTenantContext(),
            new TestCurrentBranchContext());
        var entity = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(BranchVideo))!;

        Assert.Equal("BranchVideo", entity.GetTableName());
        var uniqueOrder = entity.GetIndexes().Single(x =>
            x.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(BranchVideo.BranchId),
                nameof(BranchVideo.DisplayOrder)
            }));
        Assert.True(uniqueOrder.IsUnique);
        Assert.Contains(entity.GetIndexes(), x =>
            x.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(BranchVideo.BranchId),
                nameof(BranchVideo.IsActive),
                nameof(BranchVideo.ProcessingStatus),
                nameof(BranchVideo.DisplayOrder)
            }));

        var branchForeignKey = entity.GetForeignKeys().Single(x =>
            x.Properties.Single().Name == nameof(BranchVideo.BranchId));
        Assert.Equal(DeleteBehavior.Restrict, branchForeignKey.DeleteBehavior);

        var rowVersion = entity.FindProperty(nameof(BranchVideo.RowVersion))!;
        Assert.True(rowVersion.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, rowVersion.ValueGenerated);
        Assert.Equal(260, entity.FindProperty(nameof(BranchVideo.OriginalFileName))!.GetMaxLength());
        Assert.Equal(500, entity.FindProperty(nameof(BranchVideo.OriginalPath))!.GetMaxLength());
        Assert.Equal(500, entity.FindProperty(nameof(BranchVideo.HlsManifestPath))!.GetMaxLength());
    }
}
