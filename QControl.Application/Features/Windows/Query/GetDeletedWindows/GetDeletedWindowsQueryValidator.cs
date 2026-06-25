using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Windows.Query.GetDeletedWindows;

internal sealed class GetDeletedWindowsQueryValidator
    : AbstractValidator<GetDeletedWindowsQuery>
{
    public GetDeletedWindowsQueryValidator()
    {
        RuleFor(x => x.WaitingAreaId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Window_WaitingAreaId_Required)
            .When(x => x.WaitingAreaId.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.Window_Pagination_PageNumber_Invalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.Window_Pagination_PageSize_Invalid)
            .LessThanOrEqualTo(100)
            .WithMessage(ErrorMessage.Window_Pagination_PageSize_Max);
    }
}
