using FluentValidation;

namespace QControl.Application.Features.TicketRuntime;

internal sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator() { RuleFor(x => x.BranchId).GreaterThan(0); RuleFor(x => x.ServiceId).GreaterThan(0); RuleFor(x => x.SegmentId).GreaterThan(0); RuleFor(x => x.Field).MaximumLength(3000); RuleForEach(x => x.CustomInputs).ChildRules(x => { x.RuleFor(v => v.ServiceCustomInputId).GreaterThan(0); x.RuleFor(v => v.Value).NotNull().MaximumLength(3000); }); }
}
internal sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator() { RuleFor(x => x.BranchId).GreaterThan(0); RuleFor(x => x.ServiceId).GreaterThan(0); RuleFor(x => x.SegmentId).GreaterThan(0); RuleFor(x => x.ScheduledOnUtc).NotEmpty(); RuleFor(x => x.Field).MaximumLength(3000); RuleForEach(x => x.CustomInputs).ChildRules(x => { x.RuleFor(v => v.ServiceCustomInputId).GreaterThan(0); x.RuleFor(v => v.Value).NotNull().MaximumLength(3000); }); }
}
internal sealed class CancelTicketCommandValidator : AbstractValidator<CancelTicketCommand> { public CancelTicketCommandValidator() { RuleFor(x => x.BranchId).GreaterThan(0); RuleFor(x => x.TicketId).GreaterThan(0); RuleFor(x => x.Reason).NotEmpty().MaximumLength(500); } }
internal sealed class CancelReservationCommandValidator : AbstractValidator<CancelReservationCommand> { public CancelReservationCommandValidator() { RuleFor(x => x.BranchId).GreaterThan(0); RuleFor(x => x.ReservationId).GreaterThan(0); RuleFor(x => x.Reason).NotEmpty().MaximumLength(500); } }
internal sealed class ManualTransferTicketCommandValidator : AbstractValidator<ManualTransferTicketCommand> { public ManualTransferTicketCommandValidator() { RuleFor(x => x.BranchId).GreaterThan(0); RuleFor(x => x.TicketId).GreaterThan(0); RuleFor(x => x.TargetServiceId).GreaterThan(0); RuleFor(x => x.Reason).NotEmpty().MaximumLength(500); } }
internal sealed class GetTicketsQueryValidator : AbstractValidator<GetTicketsQuery> { public GetTicketsQueryValidator() { RuleFor(x => x.BranchId).GreaterThan(0); RuleFor(x => x.PageNumber).GreaterThan(0); RuleFor(x => x.PageSize).InclusiveBetween(1, 100); } }
internal sealed class GetReservationsQueryValidator : AbstractValidator<GetReservationsQuery> { public GetReservationsQueryValidator() { RuleFor(x => x.BranchId).GreaterThan(0); RuleFor(x => x.PageNumber).GreaterThan(0); RuleFor(x => x.PageSize).InclusiveBetween(1, 100); } }
internal sealed class GetArchivedTicketsQueryValidator : AbstractValidator<GetArchivedTicketsQuery> { public GetArchivedTicketsQueryValidator() { RuleFor(x => x.BranchId).GreaterThan(0); RuleFor(x => x.PageNumber).GreaterThan(0); RuleFor(x => x.PageSize).InclusiveBetween(1, 100); } }
internal sealed class GetArchivedReservationsQueryValidator : AbstractValidator<GetArchivedReservationsQuery> { public GetArchivedReservationsQueryValidator() { RuleFor(x => x.BranchId).GreaterThan(0); RuleFor(x => x.PageNumber).GreaterThan(0); RuleFor(x => x.PageSize).InclusiveBetween(1, 100); } }
