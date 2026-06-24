using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreas;

internal sealed class GetWaitingAreasQueryValidator
    : AbstractValidator<GetWaitingAreasQuery>
{
    public GetWaitingAreasQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.WaitingArea_BranchId_Required)
            .When(x => x.BranchId.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(
                ErrorMessage.WaitingArea_Pagination_PageNumber_Invalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(
                ErrorMessage.WaitingArea_Pagination_PageSize_Invalid)
            .LessThanOrEqualTo(100)
            .WithMessage(
                ErrorMessage.WaitingArea_Pagination_PageSize_Max);
    }
}
