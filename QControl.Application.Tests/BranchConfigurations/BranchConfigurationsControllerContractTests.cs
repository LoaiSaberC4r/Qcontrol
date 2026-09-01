using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using BuildingBlock.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Contracts.BranchConfigurations;
using Qcontrol.Api.Controllers;
using Qcontrol.Application.Features.BranchConfigurations.Command.CreateBranchConfiguration;
using Qcontrol.Application.Features.BranchConfigurations.Command.UpdateBranchConfiguration;
using Qcontrol.Application.Features.BranchConfigurations.Query.GetBranchConfiguration;
using Qcontrol.Application.Features.BranchConfigurations.Shared;
using QControl.Api.Attribute;
using QControl.infrastructure.Persistence;

namespace QControl.Application.Tests.BranchConfigurations;

public sealed class BranchConfigurationsControllerContractTests
{
    [Fact]
    public void Controller_exposes_confirmed_authenticated_route()
    {
        var controller = typeof(BranchConfigurationsController);

        Assert.Equal(
            "api/branches/{branchId:int}/configuration",
            controller.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(controller.GetCustomAttribute<ApiControllerAttribute>());
        Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Null(controller.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Theory]
    [InlineData(nameof(BranchConfigurationsController.Get),
        typeof(HttpGetAttribute), "PERMISSION_BranchConfigurations.View")]
    [InlineData(nameof(BranchConfigurationsController.Create),
        typeof(HttpPostAttribute), "PERMISSION_BranchConfigurations.Create")]
    [InlineData(nameof(BranchConfigurationsController.Update),
        typeof(HttpPutAttribute), "PERMISSION_BranchConfigurations.Update")]
    public void Actions_use_confirmed_methods_and_permissions(
        string methodName,
        Type httpAttributeType,
        string expectedPolicy)
    {
        var method = typeof(BranchConfigurationsController).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttribute(httpAttributeType));
        Assert.Equal(
            expectedPolicy,
            method.GetCustomAttribute<PermissionAttribute>()?.Policy);
        Assert.Null(method.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public async Task Controller_dispatches_route_id_and_request_contracts()
    {
        var sender = new CapturingSender();
        var controller = new BranchConfigurationsController(sender);

        Assert.IsType<OkObjectResult>(await controller.Get(
            25,
            CancellationToken.None));
        Assert.Equal(
            25,
            Assert.IsType<GetBranchConfigurationQuery>(sender.Request)
                .BranchId);

        Assert.IsType<OkObjectResult>(await controller.Create(
            25,
            new CreateBranchConfigurationRequest
            {
                AllowedTime = TimeSpan.FromMinutes(30)
            },
            CancellationToken.None));
        var create =
            Assert.IsType<CreateBranchConfigurationCommand>(sender.Request);
        Assert.Equal(25, create.BranchId);
        Assert.Equal(TimeSpan.FromMinutes(30), create.AllowedTime);

        Assert.IsType<OkObjectResult>(await controller.Update(
            25,
            new UpdateBranchConfigurationRequest
            {
                AllowedTime = TimeSpan.FromMinutes(45),
                RowVersion = "AQIDBAUGBwg="
            },
            CancellationToken.None));
        var update =
            Assert.IsType<UpdateBranchConfigurationCommand>(sender.Request);
        Assert.Equal(25, update.BranchId);
        Assert.Equal(TimeSpan.FromMinutes(45), update.AllowedTime);
        Assert.Equal("AQIDBAUGBwg=", update.RowVersion);
    }

    [Fact]
    public void Response_contract_includes_ticket_runtime_settings()
    {
        var properties = typeof(BranchConfigurationResponse)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "AllowedTime",
                "BranchId",
                "IsConfigured",
                "MaximumTicketCallAttempts",
                "Message",
                "RowVersion",
                "TicketArchiveRetentionDays",
                "TicketNoShowAutoCancellationMinutes"
            },
            properties);
    }

    [Fact]
    public void Stable_permissions_and_proven_role_assignments_are_restored()
    {
        var infrastructureAssembly = typeof(PlatformWriteDbContext).Assembly;
        var catalogType = infrastructureAssembly.GetType(
            "Qcontrol.infrastructure.Seeders.SeedConstants+SeedCatalog",
            throwOnError: true)!;

        var permissions = ReadItems(catalogType, "Permissions")
            .ToDictionary(
                item => (string)GetProperty(item, "Name"),
                item => (Guid)GetProperty(item, "Id"),
                StringComparer.Ordinal);
        var expected = new Dictionary<string, Guid>
        {
            ["BranchConfigurations.View"] = Guid.Parse(
                "30000000-0000-0000-0000-000000000096"),
            ["BranchConfigurations.Create"] = Guid.Parse(
                "30000000-0000-0000-0000-000000000097"),
            ["BranchConfigurations.Update"] = Guid.Parse(
                "30000000-0000-0000-0000-000000000098")
        };

        foreach (var permission in expected)
        {
            Assert.Equal(permission.Value, permissions[permission.Key]);
        }

        var rolePermissions = ReadItems(catalogType, "RolePermissions")
            .Select(item => (
                RoleId: (Guid)GetProperty(item, "RoleId"),
                PermissionId: (Guid)GetProperty(item, "PermissionId")))
            .ToHashSet();
        var provenRoles = new[]
        {
            Guid.Parse("20000000-0000-0000-0000-000000000001"),
            Guid.Parse("20000000-0000-0000-0000-000000000002")
        };

        foreach (var roleId in provenRoles)
        foreach (var permissionId in expected.Values)
        {
            Assert.Contains((roleId, permissionId), rolePermissions);
        }
    }

    [Fact]
    public void Ticket_issuable_services_endpoint_remains_anonymous()
    {
        var method = typeof(BranchesController).GetMethod(
            nameof(BranchesController.GetTicketIssuableServices),
            BindingFlags.Instance | BindingFlags.Public)!;

        Assert.NotNull(method.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Null(method.GetCustomAttribute<PermissionAttribute>());
    }

    private static IEnumerable<object> ReadItems(
        Type catalogType,
        string propertyName) =>
        Assert.IsAssignableFrom<IEnumerable>(catalogType.GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Static)!
            .GetValue(null))
            .Cast<object>();

    private static object GetProperty(object item, string propertyName) =>
        item.GetType().GetProperty(propertyName)!.GetValue(item)!;

    private sealed class CapturingSender : ISender
    {
        public object? Request { get; private set; }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            object response = Result<BranchConfigurationResponse>.Ok(new()
            {
                BranchId = 25,
                AllowedTime = TimeSpan.FromMinutes(30),
                IsConfigured = true,
                RowVersion = "AQIDBAUGBwg="
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
            [EnumeratorCancellation] CancellationToken cancellationToken =
                default)
        {
            await Task.CompletedTask;
            yield break;
        }

        public async IAsyncEnumerable<object?> CreateStream(
            object request,
            [EnumeratorCancellation] CancellationToken cancellationToken =
                default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
