using FluentValidation;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflowById;

internal sealed class GetServiceWorkflowByIdQueryValidator
    : AbstractValidator<GetServiceWorkflowByIdQuery>
{
    public GetServiceWorkflowByIdQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.BranchIdRequired);

        RuleFor(x => x.LeafServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.LeafServiceIdRequired);

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.IdRequired);
    }
}
