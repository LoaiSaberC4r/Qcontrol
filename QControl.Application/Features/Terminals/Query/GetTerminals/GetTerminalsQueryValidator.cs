using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Terminals.Query.GetTerminals;

internal sealed class GetTerminalsQueryValidator
    : AbstractValidator<GetTerminalsQuery>
{
    public GetTerminalsQueryValidator()
    {
        RuleFor(x => x.WindowId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Terminal_WindowId_Required)
            .When(x => x.WindowId.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.Terminal_Pagination_PageNumber_Invalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.Terminal_Pagination_PageSize_Invalid)
            .LessThanOrEqualTo(100)
            .WithMessage(ErrorMessage.Terminal_Pagination_PageSize_Max);

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.SearchTerm_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}
