using FluentValidation;
using Qcontrol.Application.Features.BranchVideos.Shared;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchVideos.Command.AddBranchVideo;

internal sealed class AddBranchVideoCommandValidator
    : AbstractValidator<AddBranchVideoCommand>
{
    public AddBranchVideoCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);
        RuleFor(x => x.Video)
            .NotNull()
            .WithMessage(BranchVideoMessages.VideoRequired);
        RuleFor(x => x.DisplayOrder)
            .GreaterThan(0)
            .WithMessage(BranchVideoMessages.InvalidDisplayOrder);
    }
}
