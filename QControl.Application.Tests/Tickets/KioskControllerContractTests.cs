using System.Reflection;
using System.Runtime.CompilerServices;
using BuildingBlock.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qcontrol.Api.Controllers;
using QControl.Api.Attribute;
using QControl.Api.Contracts.Kiosk;
using QControl.Api.Contracts.Tickets;
using QControl.Application.Features.TicketRuntime;
using QControl.Application.Features.TicketRuntime.Shared;

namespace QControl.Application.Tests.Tickets;

public sealed class KioskControllerContractTests
{
    [Fact]
    public void Kiosk_routes_are_protected_and_reuse_existing_permissions()
    {
        var controller = typeof(KioskController);
        Assert.Equal("api/branches/{branchId:int}/kiosk",
            controller.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());

        AssertAction(
            nameof(KioskController.SearchReservations),
            "reservations/search",
            "Reservations.View");
        AssertAction(
            nameof(KioskController.CreateTicketFromReservation),
            "reservations/{reservationId:int}/ticket",
            "Reservations.ConvertToTicket");
        AssertAction(
            nameof(KioskController.CreateTicket),
            "tickets",
            "Tickets.Create");
    }

    [Fact]
    public async Task Search_maps_route_and_lookup_contract_to_query()
    {
        var sender = new CapturingSender();
        var controller = new KioskController(sender);

        await controller.SearchReservations(
            7,
            new KioskReservationSearchRequest(
                15,
                null,
                new[] { new CustomInputRequest(8, "01012345678") }),
            CancellationToken.None);

        var query = Assert.IsType<SearchKioskReservationsQuery>(sender.Request);
        Assert.Equal(7, query.BranchId);
        Assert.Equal(15, query.ServiceId);
        var input = Assert.Single(query.CustomInputs);
        Assert.Equal(8, input.ServiceCustomInputId);
        Assert.Equal("01012345678", input.Value);
    }

    [Fact]
    public async Task Direct_ticket_maps_field_without_creating_reservation_command()
    {
        var sender = new CapturingSender();
        var controller = new KioskController(sender);

        await controller.CreateTicket(
            7,
            new CreateKioskTicketRequest(
                15, 2, "REF-42", Array.Empty<CustomInputRequest>()),
            CancellationToken.None);

        var command = Assert.IsType<CreateKioskTicketCommand>(sender.Request);
        Assert.Equal(7, command.BranchId);
        Assert.Equal(15, command.ServiceId);
        Assert.Equal(2, command.SegmentId);
        Assert.Equal("REF-42", command.Field);
        Assert.Empty(command.CustomInputs);
    }

    private static void AssertAction(string methodName, string route, string permission)
    {
        var method = typeof(KioskController).GetMethod(methodName);
        Assert.NotNull(method);
        Assert.Equal(route, method!.GetCustomAttribute<HttpPostAttribute>()?.Template);
        Assert.Equal($"PERMISSION_{permission}",
            method.GetCustomAttribute<PermissionAttribute>()?.Policy);
        Assert.Null(method.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    private sealed class CapturingSender : ISender
    {
        public object? Request { get; private set; }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            object result = request switch
            {
                SearchKioskReservationsQuery =>
                    Result<IReadOnlyList<KioskReservationSearchItemResponse>>.Ok(
                        Array.Empty<KioskReservationSearchItemResponse>()),
                CreateKioskTicketCommand =>
                    Result<TicketDetailsResponse>.Ok(null!),
                CreateKioskTicketFromReservationCommand =>
                    Result<TicketDetailsResponse>.Ok(null!),
                _ => throw new NotSupportedException()
            };
            return Task.FromResult((TResponse)result);
        }

        public Task<object?> Send(object request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest => throw new NotSupportedException();

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
