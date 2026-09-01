using FluentValidation;

namespace QControl.Application.Features.TicketRuntime;

internal sealed class SearchKioskReservationsQueryValidator
    : AbstractValidator<SearchKioskReservationsQuery>
{
    public SearchKioskReservationsQueryValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ServiceId).GreaterThan(0);
        RuleFor(x => x.Field).MaximumLength(3000);
        RuleForEach(x => x.CustomInputs).ChildRules(input =>
        {
            input.RuleFor(x => x.ServiceCustomInputId).GreaterThan(0);
            input.RuleFor(x => x.Value).NotEmpty().MaximumLength(3000);
        });
    }
}

internal sealed class CreateKioskTicketCommandValidator
    : AbstractValidator<CreateKioskTicketCommand>
{
    public CreateKioskTicketCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ServiceId).GreaterThan(0);
        RuleFor(x => x.SegmentId).GreaterThan(0);
        RuleFor(x => x.Field).MaximumLength(3000);
        RuleForEach(x => x.CustomInputs).ChildRules(input =>
        {
            input.RuleFor(x => x.ServiceCustomInputId).GreaterThan(0);
            input.RuleFor(x => x.Value).NotNull().MaximumLength(3000);
        });
    }
}

internal sealed class CreateKioskTicketFromReservationCommandValidator
    : AbstractValidator<CreateKioskTicketFromReservationCommand>
{
    public CreateKioskTicketFromReservationCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ReservationId).GreaterThan(0);
    }
}
