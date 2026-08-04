using FluentValidation;
using Qcontrol.Application.Features.BranchBranding.Shared;
using Qcontrol.Application.Features.GeneralBrand.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.GeneralBrand.Command.UpdateGeneralBrand;

internal sealed class UpdateGeneralBrandCommandValidator
    : AbstractValidator<UpdateGeneralBrandCommand>
{
    public UpdateGeneralBrandCommandValidator()
    {
        this.AddBrandingLayoutRules(
            GeneralBrandFeatureMessages.ValidationMessages());

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage(GeneralBrandFeatureMessages.InvalidRowVersion)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(GeneralBrandFeatureMessages.InvalidRowVersion);
    }
}
