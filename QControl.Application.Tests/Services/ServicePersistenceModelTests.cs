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

    [Fact]
    public void Model_configures_restricted_service_custom_inputs()
    {
        using var context = CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var customInput = model.FindEntityType(typeof(ServiceCustomInput))!;

        Assert.Equal("ServiceCustomInput", customInput.GetTableName());
        Assert.Equal(
            100,
            customInput.FindProperty(nameof(ServiceCustomInput.Name))!
                .GetMaxLength());
        Assert.Equal(
            200,
            customInput.FindProperty(nameof(ServiceCustomInput.LabelEn))!
                .GetMaxLength());
        Assert.Equal(
            100,
            customInput.FindProperty(nameof(ServiceCustomInput.StartWith))!
                .GetMaxLength());

        var serviceForeignKey = customInput.GetForeignKeys().Single(x =>
            x.PrincipalEntityType.ClrType == typeof(Service));
        Assert.Equal(DeleteBehavior.Restrict, serviceForeignKey.DeleteBehavior);

        var indexes = customInput.GetIndexes().ToDictionary(
            x => x.GetDatabaseName()!);
        Assert.Contains("IX_ServiceCustomInput_ServiceId", indexes.Keys);
        Assert.Contains(
            "IX_ServiceCustomInput_ServiceId_IsActive_Order",
            indexes.Keys);
        var activeNameIndex = indexes[
            "UX_ServiceCustomInput_ServiceId_Name_Active"];
        Assert.True(activeNameIndex.IsUnique);
        Assert.Equal("[IsActive] = 1", activeNameIndex.GetFilter());

        var constraintNames = customInput.GetCheckConstraints()
            .Select(x => x.Name)
            .ToHashSet();
        Assert.Contains("CK_ServiceCustomInput_Order_Positive", constraintNames);
        Assert.Contains("CK_ServiceCustomInput_Type_Valid", constraintNames);
        Assert.Contains(
            "CK_ServiceCustomInput_String_Restrictions",
            constraintNames);
        Assert.Contains(
            "CK_ServiceCustomInput_Integer_Restrictions",
            constraintNames);
        Assert.Contains(
            "CK_ServiceCustomInput_Length_Range",
            constraintNames);
        Assert.Contains(
            "CK_ServiceCustomInput_Value_Range",
            constraintNames);
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
