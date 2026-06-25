using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Displays.Query.GetDeletedDisplays;

internal sealed class GetDeletedDisplaysQueryValidator
    : AbstractValidator<GetDeletedDisplaysQuery>
{
    public GetDeletedDisplaysQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Display_BranchId_Required)
            .When(x => x.BranchId.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.Display_Pagination_PageNumber_Invalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.Display_Pagination_PageSize_Invalid)
            .LessThanOrEqualTo(100)
            .WithMessage(ErrorMessage.Display_Pagination_PageSize_Max);
    }
}
