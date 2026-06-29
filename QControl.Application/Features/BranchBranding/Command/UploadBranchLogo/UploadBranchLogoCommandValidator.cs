using FluentValidation;
using QControl.Application.Shared.Operational;
using QControl.Application.Shared.Validation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchBranding.Command.UploadBranchLogo;

internal sealed class UploadBranchLogoCommandValidator
    : AbstractValidator<UploadBranchLogoCommand>
{
    public UploadBranchLogoCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);

        RuleFor(x => x.Logo)
            .Custom((file, context) =>
            {
                var error = BranchImageFileValidator.Validate(
                file,
                "BranchBranding.LogoRequired",
                BranchFeatureMessages.LogoRequired,
                "BranchBranding.InvalidImageType",
                    BranchFeatureMessages.BrandingInvalidImageType);

                if (error is not null)
                {
                    context.AddFailure(error.Message);
                }
            });

        When(x => !string.IsNullOrWhiteSpace(x.RowVersion), () =>
        {
            RuleFor(x => x.RowVersion)
                .Must(value => RowVersionConverter.TryDecode(value, out _))
                .WithMessage(ErrorMessage.RowVersion_Invalid);
        });
    }
}
