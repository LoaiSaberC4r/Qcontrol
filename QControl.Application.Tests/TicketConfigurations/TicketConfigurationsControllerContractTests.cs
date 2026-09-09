using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using BuildingBlock.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Controllers;
using QControl.Api.Attribute;
using QControl.Api.Contracts.TicketConfigurations;
using QControl.Application.Features.TicketConfigurations.Command.CreateTicketConfiguration;
using QControl.Application.Features.TicketConfigurations.Command.UpdateTicketConfiguration;
using QControl.Application.Features.TicketConfigurations.Query.GetTicketConfiguration;
using QControl.Application.Features.TicketConfigurations.Shared;
using QControl.Domain.Enums;
using QControl.infrastructure.Persistence;

namespace QControl.Application.Tests.TicketConfigurations;

public sealed class TicketConfigurationsControllerContractTests
{
    [Fact]
    public void Controller_exposes_only_confirmed_branch_scoped_operations()
    {
        var type = typeof(TicketConfigurationsController);
        Assert.Equal("api/branches/{branchId:int}/ticket-configuration",
            type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(type.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal(3, type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Count(x => x.DeclaringType == type));

        AssertAction(nameof(TicketConfigurationsController.Get), typeof(HttpGetAttribute),
            "PERMISSION_TicketConfigurations.View");
        AssertAction(nameof(TicketConfigurationsController.Create), typeof(HttpPostAttribute),
            "PERMISSION_TicketConfigurations.Create");
        AssertAction(nameof(TicketConfigurationsController.Update), typeof(HttpPutAttribute),
            "PERMISSION_TicketConfigurations.Update");
    }

    [Fact]
    public async Task Controller_maps_complete_create_and_update_contracts()
    {
        var sender = new CapturingSender();
        var controller = new TicketConfigurationsController(sender);
        var element = new TicketPrintElementRequest
        {
            ElementType = TicketPrintElementType.BranchName,
            IsVisible = true,
            XMm = 1,
            YMm = 2,
            WidthMm = 3,
            HeightMm = 4,
            FontSizePt = 12,
            FontWeight = TicketFontWeight.Bold,
            TextAlign = TicketTextAlign.Center,
            Language = TicketPrintLanguage.Ar
        };

        await controller.Create(7, new TicketConfigurationRequest
        {
            TicketWidthMm = 80,
            TicketHeightMm = 120,
            Elements = new[] { element }
        }, CancellationToken.None);
        var create = Assert.IsType<CreateTicketConfigurationCommand>(sender.Request);
        Assert.Equal(7, create.BranchId);
        Assert.Equal(80, create.TicketWidthMm);
        Assert.Equal(TicketPrintLanguage.Ar, Assert.Single(create.Elements).Language);

        await controller.Update(7, new UpdateTicketConfigurationRequest
        {
            TicketWidthMm = 80,
            TicketHeightMm = 120,
            Elements = new[] { element },
            RowVersion = "AQIDBAUGBwg="
        }, CancellationToken.None);
        var update = Assert.IsType<UpdateTicketConfigurationCommand>(sender.Request);
        Assert.Equal("AQIDBAUGBwg=", update.RowVersion);

        await controller.Get(7, CancellationToken.None);
        Assert.Equal(7, Assert.IsType<GetTicketConfigurationQuery>(sender.Request).BranchId);
    }

    [Fact]
    public void Permissions_are_seeded_for_technical_and_branch_administrators()
    {
        var catalog = typeof(PlatformWriteDbContext).Assembly.GetType(
            "Qcontrol.infrastructure.Seeders.SeedConstants+SeedCatalog", true)!;
        var permissions = ReadItems(catalog, "Permissions").ToDictionary(
            x => (string)Get(x, "Name"), x => (Guid)Get(x, "Id"));
        var expected = new Dictionary<string, Guid>
        {
            ["TicketConfigurations.View"] = Guid.Parse("30000000-0000-0000-0000-000000000114"),
            ["TicketConfigurations.Create"] = Guid.Parse("30000000-0000-0000-0000-000000000115"),
            ["TicketConfigurations.Update"] = Guid.Parse("30000000-0000-0000-0000-000000000116")
        };
        foreach (var item in expected)
        {
            Assert.Equal(item.Value, permissions[item.Key]);
        }

        var assignments = ReadItems(catalog, "RolePermissions").Select(x => (
            (Guid)Get(x, "RoleId"), (Guid)Get(x, "PermissionId"))).ToHashSet();
        foreach (var role in new[]
                 {
                     Guid.Parse("20000000-0000-0000-0000-000000000001"),
                     Guid.Parse("20000000-0000-0000-0000-000000000002")
                 })
        foreach (var permission in expected.Values)
        {
            Assert.Contains((role, permission), assignments);
        }
    }

    [Fact]
    public void Snapshot_entities_preserve_nullable_order()
    {
        var now = DateTime.UtcNow;
        var reservation = QControl.Domain.Entities.ReservationCustomInputValue.Create(
            5, "name", "label", "عنوان", ServiceCustomInputType.String, "value", now, 9);
        var ticket = QControl.Domain.Entities.TicketCustomInputValue.Create(
            reservation.ServiceCustomInputId, reservation.NameSnapshot,
            reservation.LabelEnSnapshot, reservation.LabelArSnapshot,
            reservation.TypeSnapshot, reservation.Value, now, reservation.OrderSnapshot);

        Assert.Equal(9, reservation.OrderSnapshot);
        Assert.Equal(reservation.OrderSnapshot, ticket.OrderSnapshot);
    }

    private static void AssertAction(string name, Type methodAttribute, string policy)
    {
        var method = typeof(TicketConfigurationsController).GetMethod(name)!;
        Assert.NotNull(method.GetCustomAttribute(methodAttribute));
        Assert.Equal(policy, method.GetCustomAttribute<PermissionAttribute>()?.Policy);
    }

    private static IEnumerable<object> ReadItems(Type catalog, string property) =>
        Assert.IsAssignableFrom<IEnumerable>(catalog.GetProperty(property,
                BindingFlags.Public | BindingFlags.Static)!.GetValue(null)).Cast<object>();

    private static object Get(object value, string property) =>
        value.GetType().GetProperty(property)!.GetValue(value)!;

    private sealed class CapturingSender : ISender
    {
        public object? Request { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            object response = Result<TicketConfigurationResponse>.Ok(new());
            return Task.FromResult((TResponse)response);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest => throw new NotSupportedException();
        public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
        public async IAsyncEnumerable<object?> CreateStream(object request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
