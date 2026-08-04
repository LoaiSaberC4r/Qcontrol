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

        this.AddBrandingLayoutRules();

        When(x => !string.IsNullOrWhiteSpace(x.RowVersion), () =>
        {
            RuleFor(x => x.RowVersion)
                .Must(value => RowVersionConverter.TryDecode(value, out _))
                .WithMessage(ErrorMessage.RowVersion_Invalid);
        });
    }
}
