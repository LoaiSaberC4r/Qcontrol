using FluentValidation;
using QControl.Application.Shared.Operational;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchAdvertisements.Command.DeleteBranchAdvertisement;

internal sealed class DeleteBranchAdvertisementCommandValidator
    : AbstractValidator<DeleteBranchAdvertisementCommand>
{
    public DeleteBranchAdvertisementCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);

        RuleFor(x => x.AdvertisementId)
            .GreaterThan(0)
            .WithMessage(BranchFeatureMessages.AdvertisementIdRequired);

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
