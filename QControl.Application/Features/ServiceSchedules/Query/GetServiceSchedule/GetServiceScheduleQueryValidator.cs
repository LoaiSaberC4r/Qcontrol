using FluentValidation;
using Qcontrol.Application.Features.ServiceSchedules.Shared;

namespace Qcontrol.Application.Features.ServiceSchedules.Query.GetServiceSchedule;

internal sealed class GetServiceScheduleQueryValidator
    : AbstractValidator<GetServiceScheduleQuery>
{
    public GetServiceScheduleQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceScheduleMessages.BranchIdRequired);

        RuleFor(x => x.LeafServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceScheduleMessages.LeafServiceIdRequired);
    }
}
