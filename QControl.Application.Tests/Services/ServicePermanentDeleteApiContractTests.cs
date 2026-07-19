using System.Reflection;
using System.Runtime.CompilerServices;
using BuildingBlock.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Controllers;
using Qcontrol.Application.Features.Services.Command.PermanentDeleteService;
using QControl.Api.Attribute;

namespace QControl.Application.Tests.Services;

public sealed class ServicePermanentDeleteApiContractTests
{
    [Fact]
    public async Task Controller_maps_route_id_and_if_match_header_to_command()
    {
        var sender = new CapturingSender();
        var controller = new ServicesController(sender);

        var actionResult = await controller.DeletePermanently(
            serviceId: 25,
            rowVersion: "AAAAAAAAB9E=",
            CancellationToken.None);

        var command = Assert.IsType<PermanentDeleteServiceCommand>(
            sender.Request);
        Assert.Equal(25, command.Id);
        Assert.Equal("AAAAAAAAB9E=", command.RowVersion);
        Assert.IsType<OkObjectResult>(actionResult);
    }

    [Fact]
    public void Controller_action_uses_permanent_route_and_permission()
    {
        var method = typeof(ServicesController).GetMethod(
            nameof(ServicesController.DeletePermanently),
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(method);

        var route = method!.GetCustomAttribute<HttpDeleteAttribute>();
        var permission = method.GetCustomAttribute<PermissionAttribute>();

        Assert.Equal("{serviceId:int}/permanent", route?.Template);
        Assert.Equal("PERMISSION_Services.DeletePermanent", permission?.Policy);
    }

    private sealed class CapturingSender : ISender
    {
        public object? Request { get; private set; }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            Request = request;

            object response = Result<PermanentDeleteServiceResponse>.Ok(
                new PermanentDeleteServiceResponse
                {
                    ServiceId = 25,
                    Message = "deleted"
                });

            return Task.FromResult((TResponse)response);
        }

        public Task<object?> Send(
            object request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

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
