using FluentValidation;
using Qcontrol.Application.Features.BranchBranding.Shared;
using QControl.Application.Shared.Operational;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchBranding.Command.UpdateBranchTheme;

internal sealed class UpdateBranchThemeCommandValidator
    : AbstractValidator<UpdateBranchThemeCommand>
{
    public UpdateBranchThemeCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);

        RuleFor(x => x.MainColor)
            .NotEmpty()
            .WithMessage(BranchFeatureMessages.InvalidColor)
            .Must(value => BranchBrandingColorNormalizer.TryNormalize(value, out _))
            .WithMessage(BranchFeatureMessages.InvalidColor);

        RuleFor(x => x.SecondaryColor)
            .NotEmpty()
            .WithMessage(BranchFeatureMessages.InvalidColor)
            .Must(value => BranchBrandingColorNormalizer.TryNormalize(value, out _))
            .WithMessage(BranchFeatureMessages.InvalidColor);

        RuleFor(x => x.BackgroundColor)
            .NotEmpty()
            .WithMessage(BranchFeatureMessages.InvalidColor)
            .Must(value => BranchBrandingColorNormalizer.TryNormalize(value, out _))
            .WithMessage(BranchFeatureMessages.InvalidColor);

        When(x => !string.IsNullOrWhiteSpace(x.RowVersion), () =>
        {
            RuleFor(x => x.RowVersion)
                .Must(value => RowVersionConverter.TryDecode(value, out _))
                .WithMessage(ErrorMessage.RowVersion_Invalid);
        });
    }
}
