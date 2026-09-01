using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using QControl.Application.Abstraction.Security;
using QControl.Application.Abstraction.Services;
using QControl.Application.Features.TicketRuntime.Shared;

namespace QControl.Application.Features.TicketRuntime;

internal sealed class TicketRuntimeRequestGuard
{
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccess;
    public TicketRuntimeRequestGuard(ICurrentUser currentUser, IBranchAccessValidator branchAccess)
    {
        _currentUser = currentUser;
        _branchAccess = branchAccess;
    }

    public Result<Guid> Ensure(int branchId, string code)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Result<Guid>.Fail(new Error($"{code}.Unauthenticated", TicketRuntimeMessages.AuthenticationRequired, ErrorType.Unauthorized));
        var access = _branchAccess.EnsureCanAccessBranch(branchId, code);
        return access.IsFailure
            ? Result<Guid>.Fail(access.Errors)
            : Result<Guid>.Ok(_currentUser.UserId.Value);
    }
}

internal sealed class CreateTicketCommandHandler : ICommandHandler<CreateTicketCommand, TicketDetailsResponse>
{
    private readonly ITicketRuntimeRepository _repository; private readonly TicketRuntimeRequestGuard _guard;
    public CreateTicketCommandHandler(ITicketRuntimeRepository repository, TicketRuntimeRequestGuard guard) { _repository = repository; _guard = guard; }
    public Task<Result<TicketDetailsResponse>> Handle(CreateTicketCommand request, CancellationToken ct)
    { var g = _guard.Ensure(request.BranchId, "Tickets.Create"); return g.IsFailure ? Task.FromResult(Result<TicketDetailsResponse>.Fail(g.Errors)) : _repository.CreateTicketAsync(request.BranchId, request.ServiceId, request.SegmentId, request.CustomInputs, g.Value, ct); }
}

internal sealed class CreateReservationCommandHandler : ICommandHandler<CreateReservationCommand, ReservationDetailsResponse>
{
    private readonly ITicketRuntimeRepository _repository; private readonly TicketRuntimeRequestGuard _guard;
    public CreateReservationCommandHandler(ITicketRuntimeRepository repository, TicketRuntimeRequestGuard guard) { _repository = repository; _guard = guard; }
    public Task<Result<ReservationDetailsResponse>> Handle(CreateReservationCommand request, CancellationToken ct)
    { var g = _guard.Ensure(request.BranchId, "Reservations.Create"); return g.IsFailure ? Task.FromResult(Result<ReservationDetailsResponse>.Fail(g.Errors)) : _repository.CreateReservationAsync(request.BranchId, request.ServiceId, request.SegmentId, request.ScheduledOnUtc, request.CustomInputs, g.Value, ct); }
}

internal sealed class CreateTicketFromReservationCommandHandler : ICommandHandler<CreateTicketFromReservationCommand, TicketDetailsResponse>
{
    private readonly ITicketRuntimeRepository _repository; private readonly TicketRuntimeRequestGuard _guard;
    public CreateTicketFromReservationCommandHandler(ITicketRuntimeRepository repository, TicketRuntimeRequestGuard guard) { _repository = repository; _guard = guard; }
    public Task<Result<TicketDetailsResponse>> Handle(CreateTicketFromReservationCommand request, CancellationToken ct)
    { var g = _guard.Ensure(request.BranchId, "Reservations.ConvertToTicket"); return g.IsFailure ? Task.FromResult(Result<TicketDetailsResponse>.Fail(g.Errors)) : _repository.CreateTicketFromReservationAsync(request.BranchId, request.ReservationId, g.Value, ct); }
}

internal sealed class CancelTicketCommandHandler : ICommandHandler<CancelTicketCommand, TicketDetailsResponse>
{
    private readonly ITicketRuntimeRepository _repository; private readonly TicketRuntimeRequestGuard _guard;
    public CancelTicketCommandHandler(ITicketRuntimeRepository repository, TicketRuntimeRequestGuard guard) { _repository = repository; _guard = guard; }
    public Task<Result<TicketDetailsResponse>> Handle(CancelTicketCommand request, CancellationToken ct)
    { var g = _guard.Ensure(request.BranchId, "Tickets.Cancel"); return g.IsFailure ? Task.FromResult(Result<TicketDetailsResponse>.Fail(g.Errors)) : _repository.CancelTicketAsync(request.BranchId, request.TicketId, request.Reason, g.Value, ct); }
}

internal sealed class ReturnNoShowTicketToWaitingCommandHandler : ICommandHandler<ReturnNoShowTicketToWaitingCommand, TicketDetailsResponse>
{
    private readonly ITicketRuntimeRepository _repository; private readonly TicketRuntimeRequestGuard _guard;
    public ReturnNoShowTicketToWaitingCommandHandler(ITicketRuntimeRepository repository, TicketRuntimeRequestGuard guard) { _repository = repository; _guard = guard; }
    public Task<Result<TicketDetailsResponse>> Handle(ReturnNoShowTicketToWaitingCommand request, CancellationToken ct)
    { var g = _guard.Ensure(request.BranchId, "Tickets.ReturnNoShowToWaiting"); return g.IsFailure ? Task.FromResult(Result<TicketDetailsResponse>.Fail(g.Errors)) : _repository.ReturnNoShowToWaitingAsync(request.BranchId, request.TicketId, g.Value, ct); }
}

internal sealed class StartTicketServiceCommandHandler : ICommandHandler<StartTicketServiceCommand, TicketDetailsResponse>
{
    private readonly ITicketRuntimeRepository _repository; private readonly TicketRuntimeRequestGuard _guard;
    public StartTicketServiceCommandHandler(ITicketRuntimeRepository repository, TicketRuntimeRequestGuard guard) { _repository = repository; _guard = guard; }
    public Task<Result<TicketDetailsResponse>> Handle(StartTicketServiceCommand request, CancellationToken ct)
    { var g = _guard.Ensure(request.BranchId, "Tickets.StartService"); return g.IsFailure ? Task.FromResult(Result<TicketDetailsResponse>.Fail(g.Errors)) : _repository.StartServiceAsync(request.BranchId, request.TicketId, g.Value, ct); }
}

internal sealed class CompleteTicketServiceCommandHandler : ICommandHandler<CompleteTicketServiceCommand, TicketDetailsResponse>
{
    private readonly ITicketRuntimeRepository _repository; private readonly TicketRuntimeRequestGuard _guard;
    public CompleteTicketServiceCommandHandler(ITicketRuntimeRepository repository, TicketRuntimeRequestGuard guard) { _repository = repository; _guard = guard; }
    public Task<Result<TicketDetailsResponse>> Handle(CompleteTicketServiceCommand request, CancellationToken ct)
    { var g = _guard.Ensure(request.BranchId, "Tickets.CompleteService"); return g.IsFailure ? Task.FromResult(Result<TicketDetailsResponse>.Fail(g.Errors)) : _repository.CompleteServiceAsync(request.BranchId, request.TicketId, g.Value, ct); }
}

internal sealed class ManualTransferTicketCommandHandler : ICommandHandler<ManualTransferTicketCommand, TicketDetailsResponse>
{
    private readonly ITicketRuntimeRepository _repository; private readonly TicketRuntimeRequestGuard _guard;
    public ManualTransferTicketCommandHandler(ITicketRuntimeRepository repository, TicketRuntimeRequestGuard guard) { _repository = repository; _guard = guard; }
    public Task<Result<TicketDetailsResponse>> Handle(ManualTransferTicketCommand request, CancellationToken ct)
    { var g = _guard.Ensure(request.BranchId, "Tickets.ManualTransfer"); return g.IsFailure ? Task.FromResult(Result<TicketDetailsResponse>.Fail(g.Errors)) : _repository.ManualTransferAsync(request.BranchId, request.TicketId, request.TargetServiceId, request.Reason, g.Value, ct); }
}

internal sealed class CancelReservationCommandHandler : ICommandHandler<CancelReservationCommand, ReservationDetailsResponse>
{
    private readonly ITicketRuntimeRepository _repository; private readonly TicketRuntimeRequestGuard _guard;
    public CancelReservationCommandHandler(ITicketRuntimeRepository repository, TicketRuntimeRequestGuard guard) { _repository = repository; _guard = guard; }
    public Task<Result<ReservationDetailsResponse>> Handle(CancelReservationCommand request, CancellationToken ct)
    { var g = _guard.Ensure(request.BranchId, "Reservations.Cancel"); return g.IsFailure ? Task.FromResult(Result<ReservationDetailsResponse>.Fail(g.Errors)) : _repository.CancelReservationAsync(request.BranchId, request.ReservationId, request.Reason, g.Value, ct); }
}

internal abstract class RuntimeQueryHandlerBase
{
    protected readonly ITicketRuntimeRepository Repository; protected readonly TicketRuntimeRequestGuard Guard;
    protected RuntimeQueryHandlerBase(ITicketRuntimeRepository repository, TicketRuntimeRequestGuard guard) { Repository = repository; Guard = guard; }
}

internal sealed class GetTicketsQueryHandler : RuntimeQueryHandlerBase, IQueryHandler<GetTicketsQuery, Pagination<TicketListItemResponse>>
{
    public GetTicketsQueryHandler(ITicketRuntimeRepository r, TicketRuntimeRequestGuard g) : base(r, g) { }
    public Task<Result<Pagination<TicketListItemResponse>>> Handle(GetTicketsQuery q, CancellationToken ct) { var g = Guard.Ensure(q.BranchId, "Tickets.View"); return g.IsFailure ? Task.FromResult(Result<Pagination<TicketListItemResponse>>.Fail(g.Errors)) : Repository.GetTicketsAsync(q.BranchId, q.PageNumber, q.PageSize, q.Status, q.ServiceId, q.SegmentId, q.TicketNumber, q.BusinessDate, ct); }
}
internal sealed class GetTicketByIdQueryHandler : RuntimeQueryHandlerBase, IQueryHandler<GetTicketByIdQuery, TicketDetailsResponse>
{ public GetTicketByIdQueryHandler(ITicketRuntimeRepository r, TicketRuntimeRequestGuard g) : base(r, g) { } public Task<Result<TicketDetailsResponse>> Handle(GetTicketByIdQuery q, CancellationToken ct) { var g = Guard.Ensure(q.BranchId, "Tickets.View"); return g.IsFailure ? Task.FromResult(Result<TicketDetailsResponse>.Fail(g.Errors)) : Repository.GetTicketAsync(q.BranchId, q.TicketId, ct); } }
internal sealed class GetTicketHistoryQueryHandler : RuntimeQueryHandlerBase, IQueryHandler<GetTicketHistoryQuery, IReadOnlyList<TicketHistoryResponse>>
{ public GetTicketHistoryQueryHandler(ITicketRuntimeRepository r, TicketRuntimeRequestGuard g) : base(r, g) { } public Task<Result<IReadOnlyList<TicketHistoryResponse>>> Handle(GetTicketHistoryQuery q, CancellationToken ct) { var g = Guard.Ensure(q.BranchId, "Tickets.ViewHistory"); return g.IsFailure ? Task.FromResult(Result<IReadOnlyList<TicketHistoryResponse>>.Fail(g.Errors)) : Repository.GetTicketHistoryAsync(q.BranchId, q.TicketId, ct); } }
internal sealed class GetArchivedTicketsQueryHandler : RuntimeQueryHandlerBase, IQueryHandler<GetArchivedTicketsQuery, Pagination<TicketListItemResponse>>
{ public GetArchivedTicketsQueryHandler(ITicketRuntimeRepository r, TicketRuntimeRequestGuard g) : base(r, g) { } public Task<Result<Pagination<TicketListItemResponse>>> Handle(GetArchivedTicketsQuery q, CancellationToken ct) { var g = Guard.Ensure(q.BranchId, "Tickets.ViewArchive"); return g.IsFailure ? Task.FromResult(Result<Pagination<TicketListItemResponse>>.Fail(g.Errors)) : Repository.GetArchivedTicketsAsync(q.BranchId, q.PageNumber, q.PageSize, q.Status, q.ServiceId, q.SegmentId, q.TicketNumber, q.BusinessDate, ct); } }
internal sealed class GetArchivedTicketByIdQueryHandler : RuntimeQueryHandlerBase, IQueryHandler<GetArchivedTicketByIdQuery, TicketDetailsResponse>
{ public GetArchivedTicketByIdQueryHandler(ITicketRuntimeRepository r, TicketRuntimeRequestGuard g) : base(r, g) { } public Task<Result<TicketDetailsResponse>> Handle(GetArchivedTicketByIdQuery q, CancellationToken ct) { var g = Guard.Ensure(q.BranchId, "Tickets.ViewArchive"); return g.IsFailure ? Task.FromResult(Result<TicketDetailsResponse>.Fail(g.Errors)) : Repository.GetArchivedTicketAsync(q.BranchId, q.TicketId, ct); } }
internal sealed class GetReservationsQueryHandler : RuntimeQueryHandlerBase, IQueryHandler<GetReservationsQuery, Pagination<ReservationListItemResponse>>
{ public GetReservationsQueryHandler(ITicketRuntimeRepository r, TicketRuntimeRequestGuard g) : base(r, g) { } public Task<Result<Pagination<ReservationListItemResponse>>> Handle(GetReservationsQuery q, CancellationToken ct) { var g = Guard.Ensure(q.BranchId, "Reservations.View"); return g.IsFailure ? Task.FromResult(Result<Pagination<ReservationListItemResponse>>.Fail(g.Errors)) : Repository.GetReservationsAsync(q.BranchId, q.PageNumber, q.PageSize, q.Status, q.ServiceId, q.SegmentId, q.BusinessDate, ct); } }
internal sealed class GetReservationByIdQueryHandler : RuntimeQueryHandlerBase, IQueryHandler<GetReservationByIdQuery, ReservationDetailsResponse>
{ public GetReservationByIdQueryHandler(ITicketRuntimeRepository r, TicketRuntimeRequestGuard g) : base(r, g) { } public Task<Result<ReservationDetailsResponse>> Handle(GetReservationByIdQuery q, CancellationToken ct) { var g = Guard.Ensure(q.BranchId, "Reservations.View"); return g.IsFailure ? Task.FromResult(Result<ReservationDetailsResponse>.Fail(g.Errors)) : Repository.GetReservationAsync(q.BranchId, q.ReservationId, ct); } }
internal sealed class GetReservationHistoryQueryHandler : RuntimeQueryHandlerBase, IQueryHandler<GetReservationHistoryQuery, IReadOnlyList<ReservationHistoryResponse>>
{ public GetReservationHistoryQueryHandler(ITicketRuntimeRepository r, TicketRuntimeRequestGuard g) : base(r, g) { } public Task<Result<IReadOnlyList<ReservationHistoryResponse>>> Handle(GetReservationHistoryQuery q, CancellationToken ct) { var g = Guard.Ensure(q.BranchId, "Reservations.ViewHistory"); return g.IsFailure ? Task.FromResult(Result<IReadOnlyList<ReservationHistoryResponse>>.Fail(g.Errors)) : Repository.GetReservationHistoryAsync(q.BranchId, q.ReservationId, ct); } }
internal sealed class GetArchivedReservationsQueryHandler : RuntimeQueryHandlerBase, IQueryHandler<GetArchivedReservationsQuery, Pagination<ReservationListItemResponse>>
{ public GetArchivedReservationsQueryHandler(ITicketRuntimeRepository r, TicketRuntimeRequestGuard g) : base(r, g) { } public Task<Result<Pagination<ReservationListItemResponse>>> Handle(GetArchivedReservationsQuery q, CancellationToken ct) { var g = Guard.Ensure(q.BranchId, "Reservations.ViewArchive"); return g.IsFailure ? Task.FromResult(Result<Pagination<ReservationListItemResponse>>.Fail(g.Errors)) : Repository.GetArchivedReservationsAsync(q.BranchId, q.PageNumber, q.PageSize, q.Status, q.ServiceId, q.SegmentId, q.BusinessDate, ct); } }
internal sealed class GetArchivedReservationByIdQueryHandler : RuntimeQueryHandlerBase, IQueryHandler<GetArchivedReservationByIdQuery, ReservationDetailsResponse>
{ public GetArchivedReservationByIdQueryHandler(ITicketRuntimeRepository r, TicketRuntimeRequestGuard g) : base(r, g) { } public Task<Result<ReservationDetailsResponse>> Handle(GetArchivedReservationByIdQuery q, CancellationToken ct) { var g = Guard.Ensure(q.BranchId, "Reservations.ViewArchive"); return g.IsFailure ? Task.FromResult(Result<ReservationDetailsResponse>.Fail(g.Errors)) : Repository.GetArchivedReservationAsync(q.BranchId, q.ReservationId, ct); } }
