using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Windows.Query.GetWindows;

internal sealed class GetWindowsQueryValidator
    : AbstractValidator<GetWindowsQuery>
{
    public GetWindowsQueryValidator()
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

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.SearchTerm_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}
