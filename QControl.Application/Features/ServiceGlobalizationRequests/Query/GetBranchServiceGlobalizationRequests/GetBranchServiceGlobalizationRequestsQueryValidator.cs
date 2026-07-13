using FluentValidation;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Query.GetBranchServiceGlobalizationRequests;

internal sealed class GetBranchServiceGlobalizationRequestsQueryValidator
    : AbstractValidator<GetBranchServiceGlobalizationRequestsQuery>
{
    public GetBranchServiceGlobalizationRequestsQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceGlobalizationRequestMessages.BranchRequestAccessForbidden);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ServiceGlobalizationRequestMessages.PageNumberInvalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ServiceGlobalizationRequestMessages.PageSizeInvalid)
            .LessThanOrEqualTo(100)
            .WithMessage(ServiceGlobalizationRequestMessages.PageSizeMax);

        RuleFor(x => x.SearchText)
            .MaximumLength(200)
            .WithMessage(ServiceGlobalizationRequestMessages.SearchTextMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchText));

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue);

        RuleFor(x => x.RequestType)
            .IsInEnum()
            .When(x => x.RequestType.HasValue);
    }
}
