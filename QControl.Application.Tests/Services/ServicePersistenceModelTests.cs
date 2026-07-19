using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.Application.Tests.Services;

public sealed class ServicePersistenceModelTests
{
    [Fact]
    public void Model_configures_service_code_columns_constraint_and_index()
    {
        using var context = CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var service = model.FindEntityType(typeof(Service))!;

        var serviceCode = service.FindProperty(nameof(Service.ServiceCode))!;
        Assert.False(serviceCode.IsUnicode());
        Assert.Equal(ServiceCodeNormalizer.MaxLength, serviceCode.GetMaxLength());
        Assert.True(serviceCode.IsNullable);

        var isRequired = service.FindProperty(
            nameof(Service.IsServiceCodeRequired))!;
        Assert.False(isRequired.IsNullable);
        Assert.Equal(false, isRequired.GetDefaultValue());

        var index = service.GetIndexes().Single(x =>
            x.GetDatabaseName() == "UX_Service_ServiceCode");
        Assert.True(index.IsUnique);
        Assert.Equal("[ServiceCode] IS NOT NULL", index.GetFilter());

        var constraint = service.GetCheckConstraints().Single(x =>
            x.Name == "CK_Service_ServiceCode_Requirement");
        Assert.Contains("[IsServiceCodeRequired] = 1", constraint.Sql);
        Assert.Contains("[ServiceCode] IS NULL", constraint.Sql);
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
