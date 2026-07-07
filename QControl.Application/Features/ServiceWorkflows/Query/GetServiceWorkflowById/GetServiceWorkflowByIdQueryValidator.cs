using FluentValidation;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflowById;

internal sealed class GetServiceWorkflowByIdQueryValidator
    : AbstractValidator<GetServiceWorkflowByIdQuery>
{
    public GetServiceWorkflowByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.IdRequired);
    }
}
