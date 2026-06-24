using FluentValidation;
using Qcontrol.Domain.Resources;
using System.Net;
using System.Net.Sockets;

namespace Qcontrol.Application.Features.Branches.Command.CreateBranch;

internal sealed class CreateBranchCommandValidator
    : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.ArabicName) ||
                !string.IsNullOrWhiteSpace(x.EnglishName))
            .WithMessage(ErrorMessage.Branch_Name_Required);

        RuleFor(x => x.ArabicName)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Branch_ArabicName_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.ArabicName));

        RuleFor(x => x.EnglishName)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Branch_EnglishName_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.EnglishName));

        RuleFor(x => x.IPAddress)
            .NotEmpty()
            .WithMessage(ErrorMessage.Branch_IPAddress_Required)
            .MaximumLength(45)
            .WithMessage(ErrorMessage.Branch_IPAddress_MaxLength)
            .Must(BeValidIPAddress)
            .WithMessage(ErrorMessage.Branch_IPAddress_Invalid);

        RuleFor(x => x.Governorate)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Branch_Governorate_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Governorate));

        RuleFor(x => x.City)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Branch_City_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.Area)
            .MaximumLength(100)
            .WithMessage(ErrorMessage.Branch_Area_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Area));

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage(ErrorMessage.Branch_Address_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.Longitude)
            .MaximumLength(50)
            .WithMessage(ErrorMessage.Branch_Longitude_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Longitude));

        RuleFor(x => x.Latitude)
            .MaximumLength(50)
            .WithMessage(ErrorMessage.Branch_Latitude_MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Latitude));
    }

    private static bool BeValidIPAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Contains(':'))
        {
            return IPAddress.TryParse(normalizedValue, out var ipv6) &&
                   ipv6.AddressFamily == AddressFamily.InterNetworkV6;
        }

        var parts = normalizedValue.Split('.');

        return parts.Length == 4 &&
               parts.All(part =>
                   part.Length > 0 &&
                   byte.TryParse(part, out _));
    }
}