using FluentValidation;
using Qcontrol.Application.Features.BranchVideos.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchVideos.Command.ReorderBranchVideos;

internal sealed class ReorderBranchVideosCommandValidator
    : AbstractValidator<ReorderBranchVideosCommand>
{
    public ReorderBranchVideosCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage(BranchVideoMessages.InvalidDisplayOrder);
        RuleFor(x => x.Items)
            .Must(items => items.Select(x => x.VideoId).Distinct().Count() == items.Count)
            .When(x => x.Items.Count > 0)
            .WithMessage(BranchVideoMessages.DuplicateVideoId);
        RuleFor(x => x.Items)
            .Must(items => items.Select(x => x.DisplayOrder).Distinct().Count() == items.Count)
            .When(x => x.Items.Count > 0)
            .WithMessage(BranchVideoMessages.DuplicateDisplayOrder);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.VideoId)
                .GreaterThan(0)
                .WithMessage(BranchVideoMessages.NotFound);
            item.RuleFor(x => x.DisplayOrder)
                .GreaterThan(0)
                .WithMessage(BranchVideoMessages.InvalidDisplayOrder);
            item.RuleFor(x => x.RowVersion)
                .NotEmpty()
                .WithMessage(ErrorMessage.RowVersion_Required)
                .Must(value => RowVersionConverter.TryDecode(value, out _))
                .WithMessage(ErrorMessage.RowVersion_Invalid);
        });
    }
}
