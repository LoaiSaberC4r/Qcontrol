using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchVideos.Query.GetBranchDisplayPlaylist;

internal sealed class GetBranchDisplayPlaylistQueryValidator
    : AbstractValidator<GetBranchDisplayPlaylistQuery>
{
    public GetBranchDisplayPlaylistQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);
    }
}
