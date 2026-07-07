using FluentValidation;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflowsForService;

internal sealed class GetServiceWorkflowsForServiceQueryValidator
    : AbstractValidator<GetServiceWorkflowsForServiceQuery>
{
    public GetServiceWorkflowsForServiceQueryValidator()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.ServiceIdRequired);
    }
}
