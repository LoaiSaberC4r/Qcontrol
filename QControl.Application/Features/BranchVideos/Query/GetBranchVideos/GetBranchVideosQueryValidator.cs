using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchVideos.Query.GetBranchVideos;

internal sealed class GetBranchVideosQueryValidator
    : AbstractValidator<GetBranchVideosQuery>
{
    public GetBranchVideosQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);
    }
}
