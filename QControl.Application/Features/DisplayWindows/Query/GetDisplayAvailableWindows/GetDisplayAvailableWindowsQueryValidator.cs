using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayAvailableWindows;

internal sealed class GetDisplayAvailableWindowsQueryValidator
    : AbstractValidator<GetDisplayAvailableWindowsQuery>
{
    public GetDisplayAvailableWindowsQueryValidator()
    {
        RuleFor(x => x.DisplayId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.DisplayWindow_DisplayId_Required);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.DisplayWindow_Pagination_PageNumber_Invalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.DisplayWindow_Pagination_PageSize_Invalid)
            .LessThanOrEqualTo(100)
            .WithMessage(ErrorMessage.DisplayWindow_Pagination_PageSize_Max);
    }
}
