using FluentValidation;
using Qcontrol.Application.Features.BranchBranding.Shared;
using Qcontrol.Application.Features.GeneralBrand.Shared;

namespace Qcontrol.Application.Features.GeneralBrand.Command.CreateGeneralBrand;

internal sealed class CreateGeneralBrandCommandValidator
    : AbstractValidator<CreateGeneralBrandCommand>
{
    public CreateGeneralBrandCommandValidator()
    {
        this.AddBrandingLayoutRules(
            GeneralBrandFeatureMessages.ValidationMessages());
    }
}
