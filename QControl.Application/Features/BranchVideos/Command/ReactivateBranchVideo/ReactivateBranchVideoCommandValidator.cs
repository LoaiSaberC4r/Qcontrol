using FluentValidation;
using Qcontrol.Application.Features.BranchVideos.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchVideos.Command.ReactivateBranchVideo;

internal sealed class ReactivateBranchVideoCommandValidator
    : AbstractValidator<ReactivateBranchVideoCommand>
{
    public ReactivateBranchVideoCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage(ErrorMessage.Branch_Id_Required);
        RuleFor(x => x.VideoId).GreaterThan(0).WithMessage(BranchVideoMessages.NotFound);
        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
