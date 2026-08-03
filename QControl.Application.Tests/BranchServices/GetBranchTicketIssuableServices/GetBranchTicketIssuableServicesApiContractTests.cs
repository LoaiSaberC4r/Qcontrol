using System.Reflection;
using System.Runtime.CompilerServices;
using BuildingBlock.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Controllers;
using Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;
using QControl.Api.Attribute;

namespace QControl.Application.Tests.BranchServices
    .GetBranchTicketIssuableServices;

public sealed class GetBranchTicketIssuableServicesApiContractTests
{
    [Fact]
    public async Task Controller_maps_route_branch_id_to_query()
    {
        var sender = new CapturingSender();
        var controller = new BranchesController(sender);

        var actionResult = await controller.GetTicketIssuableServices(
            25,
            CancellationToken.None);

        var query = Assert.IsType<GetBranchTicketIssuableServicesQuery>(
            sender.Request);
        Assert.Equal(25, query.BranchId);
        Assert.IsType<OkObjectResult>(actionResult);
    }

    [Fact]
    public void Controller_action_uses_confirmed_route_and_existing_permission()
    {
        var method = typeof(BranchesController).GetMethod(
            nameof(BranchesController.GetTicketIssuableServices),
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(method);
        Assert.Equal(
            "{branchId:int}/ticket-issuable-services",
            method!.GetCustomAttribute<HttpGetAttribute>()?.Template);
        Assert.Equal(
            "PERMISSION_BranchServices.View",
            method.GetCustomAttribute<PermissionAttribute>()?.Policy);
    }

    private sealed class CapturingSender : ISender
    {
        public object? Request { get; private set; }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            Request = request;

            object response =
                Result<GetBranchTicketIssuableServicesResponse>.Ok(new()
                {
                    BranchId = 25
                });

            return Task.FromResult((TResponse)response);
        }

        public Task<object?> Send(
            object request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest =>
            throw new NotSupportedException();

        public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

        public async IAsyncEnumerable<object?> CreateStream(
            object request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
