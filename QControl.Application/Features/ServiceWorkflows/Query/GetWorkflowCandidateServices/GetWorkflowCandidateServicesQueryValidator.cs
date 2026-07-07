using FluentValidation;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowCandidateServices;

internal sealed class GetWorkflowCandidateServicesQueryValidator
    : AbstractValidator<GetWorkflowCandidateServicesQuery>
{
    public GetWorkflowCandidateServicesQueryValidator()
    {
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

        RuleFor(x => x.ParentServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceWorkflowMessages.ServiceIdRequired)
            .When(x => x.ParentServiceId.HasValue);
    }
}
