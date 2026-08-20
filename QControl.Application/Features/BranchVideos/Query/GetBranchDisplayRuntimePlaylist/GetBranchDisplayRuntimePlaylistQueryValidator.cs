using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchVideos.Query.GetBranchDisplayRuntimePlaylist;

internal sealed class GetBranchDisplayRuntimePlaylistQueryValidator
    : AbstractValidator<GetBranchDisplayRuntimePlaylistQuery>
{
    public GetBranchDisplayRuntimePlaylistQueryValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage(ErrorMessage.Branch_Id_Required);
    }
}
