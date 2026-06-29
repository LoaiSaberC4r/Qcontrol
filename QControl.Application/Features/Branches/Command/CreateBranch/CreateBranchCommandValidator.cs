using FluentValidation;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Branches.Command.CreateBranch;

internal sealed class CreateBranchCommandValidator
    : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.ArabicName)
            .NotEmpty()
            .WithMessage(ErrorMessage.Branch_ArabicName_Required)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Branch_ArabicName_MaxLength);

        RuleFor(x => x.EnglishName)
            .NotEmpty()
            .WithMessage(ErrorMessage.Branch_EnglishName_Required)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Branch_EnglishName_MaxLength);

        RuleFor(x => x.IPAddress)
            .NotEmpty()
            .WithMessage(ErrorMessage.Branch_IPAddress_Required)
            .MaximumLength(45)
            .WithMessage(ErrorMessage.Branch_IPAddress_MaxLength)
            .Must(value => IPAddressNormalizer.TryNormalize(value, out _))
            .WithMessage(ErrorMessage.Branch_IPAddress_Invalid);

        RuleFor(x => x.Governorate)
            .NotEmpty()
            .WithMessage(ErrorMessage.Branch_Governorate_Required)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Branch_Governorate_MaxLength);

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage(ErrorMessage.Branch_City_Required)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Branch_City_MaxLength);

        RuleFor(x => x.Area)
            .NotEmpty()
            .WithMessage(ErrorMessage.Branch_Area_Required)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Branch_Area_MaxLength);

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage(ErrorMessage.Branch_Address_Required)
            .MaximumLength(500)
            .WithMessage(ErrorMessage.Branch_Address_MaxLength);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m)
            .WithMessage(ErrorMessage.Branch_Latitude_Invalid);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m)
            .WithMessage(ErrorMessage.Branch_Longitude_Invalid);
    }
}
