using Qcontrol.Application.Features.Services.Query.GetServiceById;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace QControl.Application.Tests.Services;

public sealed class ServiceCustomInputQueryTests
{
    [Fact]
    public async Task Details_returns_active_inputs_in_order_with_all_restrictions()
    {
        var service = EntityTestFactory.Service(
            10,
            isClientInputRequired: true);
        EntityTestFactory.ServiceCustomInput(
            service,
            22,
            "Age",
            ServiceCustomInputType.Integer,
            order: 2,
            minValue: 18,
            maxValue: 100,
            labelEn: "Age");
        EntityTestFactory.ServiceCustomInput(
            service,
            21,
            "NationalId",
            order: 1,
            isRequired: true,
            minLength: 14,
            maxLength: 14,
            startWith: "2",
            labelAr: "الرقم القومي");
        EntityTestFactory.ServiceCustomInput(
            service,
            20,
            "Inactive",
            order: 3,
            isActive: false);

        var result = await CreateHandler(new List<Service> { service }).Handle(
            new GetServiceByIdQuery { Id = service.Id },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsClientInputRequired);
        var customInputs = result.Value.CustomInputs!;
        Assert.Equal(
            new[] { 21, 22 },
            customInputs.Select(x => x.CustomInputId));
        var stringInput = customInputs[0];
        Assert.Equal("String", stringInput.TypeName);
        Assert.Equal(14, stringInput.MinLength);
        Assert.Equal("2", stringInput.StartWith);
        Assert.Null(stringInput.MinValue);
        var integerInput = customInputs[1];
        Assert.Equal(18, integerInput.MinValue);
        Assert.Equal(100, integerInput.MaxValue);
        Assert.Null(integerInput.MaxLength);
    }

    [Fact]
    public async Task Details_returns_null_for_leaf_without_applicable_inputs()
    {
        var service = EntityTestFactory.Service(10);
        EntityTestFactory.ServiceCustomInput(service, 20, "Legacy");

        var result = await CreateHandler(new List<Service> { service }).Handle(
            new GetServiceByIdQuery { Id = service.Id },
            CancellationToken.None);

        Assert.False(result.Value.IsClientInputRequired);
        Assert.Null(result.Value.CustomInputs);
    }

    [Fact]
    public async Task Details_hides_inconsistent_parent_configuration()
    {
        var parent = EntityTestFactory.Service(
            10,
            isClientInputRequired: true);
        EntityTestFactory.ServiceCustomInput(parent, 20, "Legacy");
        var child = EntityTestFactory.Service(11, parentServiceId: parent.Id);

        var result = await CreateHandler(
            new List<Service> { parent, child }).Handle(
            new GetServiceByIdQuery { Id = parent.Id },
            CancellationToken.None);

        Assert.True(result.Value.HasChildren);
        Assert.False(result.Value.IsClientInputRequired);
        Assert.Null(result.Value.CustomInputs);
    }

    [Fact]
    public async Task Details_keeps_legacy_required_flag_stable_when_no_active_inputs()
    {
        var service = EntityTestFactory.Service(
            10,
            isClientInputRequired: true);

        var result = await CreateHandler(new List<Service> { service }).Handle(
            new GetServiceByIdQuery { Id = service.Id },
            CancellationToken.None);

        Assert.True(result.Value.IsClientInputRequired);
        Assert.Null(result.Value.CustomInputs);
    }

    private static GetServiceByIdQueryHandler CreateHandler(
        List<Service> services) => new(
            new InMemoryWriteReadRepository<Service>(services),
            new InMemoryWriteReadRepository<BranchService>(
                new List<BranchService>()),
            new InMemoryWriteReadRepository<ServiceImage>(
                new List<ServiceImage>()),
            new TestCurrentUser(),
            new TestCurrentBranchContext(),
            new TestServiceVisibilityPolicy());
}
