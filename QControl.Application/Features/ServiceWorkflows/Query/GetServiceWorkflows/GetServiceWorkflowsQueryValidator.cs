using FluentValidation;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflows;

internal sealed class GetServiceWorkflowsQueryValidator
    : AbstractValidator<GetServiceWorkflowsQuery>
{
    public GetServiceWorkflowsQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.BranchIdRequired);

        RuleFor(x => x.LeafServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.LeafServiceIdRequired);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ServiceWorkflowMessages.PageNumberInvalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ServiceWorkflowMessages.PageSizeInvalid)
            .LessThanOrEqualTo(100)
            .WithMessage(ServiceWorkflowMessages.PageSizeMax);

        RuleFor(x => x.SearchText)
            .MaximumLength(200)
            .WithMessage(ErrorMessage.SearchTerm_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchText));

    }
}
