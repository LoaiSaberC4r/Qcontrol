using FluentValidation;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Command.ReorderBranchDisplayMessages;

internal sealed class ReorderBranchDisplayMessagesCommandValidator
    : AbstractValidator<ReorderBranchDisplayMessagesCommand>
{
    public ReorderBranchDisplayMessagesCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage(ErrorMessage.Branch_Id_Required);
        RuleFor(x => x.Items).NotEmpty().WithMessage(BranchDisplayFeatureMessages.InvalidDisplayOrder);
        RuleFor(x => x.Items)
            .Must(items => items.Select(x => x.MessageId).Distinct().Count() == items.Count)
            .When(x => x.Items.Count > 0)
            .WithMessage(BranchDisplayFeatureMessages.DuplicateMessageId);
        RuleFor(x => x.Items)
            .Must(items => items.Select(x => x.DisplayOrder).Distinct().Count() == items.Count)
            .When(x => x.Items.Count > 0)
            .WithMessage(BranchDisplayFeatureMessages.DuplicateDisplayOrder);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.MessageId).GreaterThan(0).WithMessage(BranchDisplayFeatureMessages.MessageNotFound);
            item.RuleFor(x => x.DisplayOrder).GreaterThan(0).WithMessage(BranchDisplayFeatureMessages.InvalidDisplayOrder);
            item.RuleFor(x => x.RowVersion)
                .NotEmpty().WithMessage(ErrorMessage.RowVersion_Required)
                .Must(value => RowVersionConverter.TryDecode(value, out _))
                .WithMessage(ErrorMessage.RowVersion_Invalid);
        });
    }
}
