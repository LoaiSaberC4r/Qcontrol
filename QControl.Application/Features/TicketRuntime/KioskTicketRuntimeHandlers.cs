using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using QControl.Application.Abstraction.Services;
using QControl.Application.Features.TicketRuntime.Shared;

namespace QControl.Application.Features.TicketRuntime;

internal sealed class SearchKioskReservationsQueryHandler
    : IQueryHandler<SearchKioskReservationsQuery,
        IReadOnlyList<KioskReservationSearchItemResponse>>
{
    private readonly ITicketRuntimeRepository _repository;
    private readonly TicketRuntimeRequestGuard _guard;

    public SearchKioskReservationsQueryHandler(
        ITicketRuntimeRepository repository,
        TicketRuntimeRequestGuard guard)
    {
        _repository = repository;
        _guard = guard;
    }

    public Task<Result<IReadOnlyList<KioskReservationSearchItemResponse>>> Handle(
        SearchKioskReservationsQuery request,
        CancellationToken cancellationToken)
    {
        var access = _guard.Ensure(request.BranchId, "Reservations.View");
        return access.IsFailure
            ? Task.FromResult(Result<IReadOnlyList<KioskReservationSearchItemResponse>>
                .Fail(access.Errors))
            : _repository.SearchReservationsForKioskAsync(
                request.BranchId,
                request.ServiceId,
                request.Field,
                request.CustomInputs,
                cancellationToken);
    }
}

internal sealed class CreateKioskTicketCommandHandler
    : ICommandHandler<CreateKioskTicketCommand, TicketDetailsResponse>
{
    private readonly ITicketRuntimeRepository _repository;
    private readonly TicketRuntimeRequestGuard _guard;

    public CreateKioskTicketCommandHandler(
        ITicketRuntimeRepository repository,
        TicketRuntimeRequestGuard guard)
    {
        _repository = repository;
        _guard = guard;
    }

    public Task<Result<TicketDetailsResponse>> Handle(
        CreateKioskTicketCommand request,
        CancellationToken cancellationToken)
    {
        var access = _guard.Ensure(request.BranchId, "Tickets.Create");
        return access.IsFailure
            ? Task.FromResult(Result<TicketDetailsResponse>.Fail(access.Errors))
            : _repository.CreateTicketAsync(
                request.BranchId,
                request.ServiceId,
                request.SegmentId,
                request.Field,
                request.CustomInputs,
                requireLookupValueWhenNoCustomInputs: true,
                access.Value,
                cancellationToken);
    }
}

internal sealed class CreateKioskTicketFromReservationCommandHandler
    : ICommandHandler<CreateKioskTicketFromReservationCommand, TicketDetailsResponse>
{
    private readonly ITicketRuntimeRepository _repository;
    private readonly TicketRuntimeRequestGuard _guard;

    public CreateKioskTicketFromReservationCommandHandler(
        ITicketRuntimeRepository repository,
        TicketRuntimeRequestGuard guard)
    {
        _repository = repository;
        _guard = guard;
    }

    public Task<Result<TicketDetailsResponse>> Handle(
        CreateKioskTicketFromReservationCommand request,
        CancellationToken cancellationToken)
    {
        var access = _guard.Ensure(request.BranchId, "Reservations.ConvertToTicket");
        return access.IsFailure
            ? Task.FromResult(Result<TicketDetailsResponse>.Fail(access.Errors))
            : _repository.CreateTicketFromReservationAsync(
                request.BranchId,
                request.ReservationId,
                access.Value,
                cancellationToken);
    }
}
