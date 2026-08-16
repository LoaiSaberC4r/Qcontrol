using Qcontrol.Api.Contracts.Services;
using Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;
using Qcontrol.Application.Features.Services.Shared;

namespace QControl.Application.Tests.Services;

public sealed class ServiceCustomInputApiContractTests
{
    [Fact]
    public void Confirmed_service_contracts_expose_custom_inputs()
    {
        Assert.Equal(
            typeof(IReadOnlyCollection<CreateServiceCustomInputRequest>),
            typeof(CreateServiceRequest).GetProperty("CustomInputs")!
                .PropertyType);
        Assert.Equal(
            typeof(IReadOnlyCollection<UpdateServiceCustomInputRequest>),
            typeof(UpdateServiceRequest).GetProperty("CustomInputs")!
                .PropertyType);
        Assert.Equal(
            typeof(IReadOnlyList<ServiceCustomInputResponse>),
            Nullable.GetUnderlyingType(
                typeof(ServiceDetailsResponse).GetProperty("CustomInputs")!
                    .PropertyType) ??
            typeof(ServiceDetailsResponse).GetProperty("CustomInputs")!
                .PropertyType);
        Assert.NotNull(
            typeof(TicketIssuableServiceTreeNodeResponse)
                .GetProperty("IsClientInputRequired"));
        Assert.NotNull(
            typeof(TicketIssuableServiceTreeNodeResponse)
                .GetProperty("CustomInputs"));
    }

    [Fact]
    public void Update_item_keeps_nullable_custom_input_id()
    {
        Assert.Equal(
            typeof(int?),
            typeof(UpdateServiceCustomInputRequest)
                .GetProperty("CustomInputId")!
                .PropertyType);
    }
}
