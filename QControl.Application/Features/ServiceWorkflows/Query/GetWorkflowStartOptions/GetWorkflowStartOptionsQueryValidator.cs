using FluentValidation;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowStartOptions;

internal sealed class GetWorkflowStartOptionsQueryValidator
    : AbstractValidator<GetWorkflowStartOptionsQuery>
{
    public GetWorkflowStartOptionsQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.BranchIdRequired);

        RuleFor(x => x.LeafServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.LeafServiceIdRequired);
    }
}
