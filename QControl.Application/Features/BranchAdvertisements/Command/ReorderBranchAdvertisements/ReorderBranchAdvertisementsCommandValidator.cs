using FluentValidation;
using QControl.Application.Shared.Operational;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchAdvertisements.Command.ReorderBranchAdvertisements;

internal sealed class ReorderBranchAdvertisementsCommandValidator
    : AbstractValidator<ReorderBranchAdvertisementsCommand>
{
    public ReorderBranchAdvertisementsCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage(BranchFeatureMessages.IncompleteReorder);

        RuleFor(x => x.Items)
            .Must(items => items.Select(x => x.AdvertisementId).Distinct().Count() == items.Count)
            .When(x => x.Items.Count > 0)
            .WithMessage(BranchFeatureMessages.IncompleteReorder);

        RuleFor(x => x.Items)
            .Must(items => items.Select(x => x.DisplayOrder).Distinct().Count() == items.Count)
            .When(x => x.Items.Count > 0)
            .WithMessage(BranchFeatureMessages.DuplicateDisplayOrder);

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.AdvertisementId)
                    .GreaterThan(0)
                    .WithMessage(BranchFeatureMessages.AdvertisementIdRequired);

                item.RuleFor(x => x.DisplayOrder)
                    .InclusiveBetween(1, 20)
                    .WithMessage(BranchFeatureMessages.InvalidDisplayOrder);

                item.RuleFor(x => x.RowVersion)
                    .NotEmpty()
                    .WithMessage(ErrorMessage.RowVersion_Required)
                    .Must(value => RowVersionConverter.TryDecode(value, out _))
                    .WithMessage(ErrorMessage.RowVersion_Invalid);
            });
    }
}
