using FluentValidation;
using QControl.Application.Shared.Operational;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchAdvertisements.Command.AddBranchAdvertisements;

internal sealed class AddBranchAdvertisementsCommandValidator
    : AbstractValidator<AddBranchAdvertisementsCommand>
{
    public AddBranchAdvertisementsCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);

        RuleFor(x => x.Images)
            .NotEmpty()
            .WithMessage(BranchFeatureMessages.AdvertisementImagesRequired);
    }
}
