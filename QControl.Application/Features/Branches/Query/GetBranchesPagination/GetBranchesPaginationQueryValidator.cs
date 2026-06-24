using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Branches.Query.GetBranchesPagination;

internal sealed class GetBranchesPaginationQueryValidator
    : AbstractValidator<GetBranchesPaginationQuery>
{
    public GetBranchesPaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.Branch_Pagination_PageNumber_Invalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.Branch_Pagination_PageSize_Invalid)
            .LessThanOrEqualTo(100)
            .WithMessage(ErrorMessage.Branch_Pagination_PageSize_Max);
    }
}