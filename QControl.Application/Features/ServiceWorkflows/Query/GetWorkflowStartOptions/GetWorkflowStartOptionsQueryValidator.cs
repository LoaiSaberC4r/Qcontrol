using FluentValidation;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowStartOptions;

internal sealed class GetWorkflowStartOptionsQueryValidator
    : AbstractValidator<GetWorkflowStartOptionsQuery>
{
    public GetWorkflowStartOptionsQueryValidator()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.ServiceIdRequired);
    }
}
