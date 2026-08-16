using BuildingBlock.Domain.Results;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Services.Shared;

internal static class ServiceCustomInputRuleChecks
{
    public static Error? Validate(
        IReadOnlyCollection<ServiceCustomInputDefinitionCommand>? customInputs,
        bool isClientInputRequired,
        string operation,
        bool validateIds)
    {
        if (!isClientInputRequired)
        {
            return customInputs is { Count: > 0 }
                ? Error(
                    operation,
                    "CustomInputsNotAllowed",
                    ServiceFeatureMessages.CustomInputsNotAllowed)
                : null;
        }

        if (customInputs is null || customInputs.Count == 0)
        {
            return Error(
                operation,
                "CustomInputsRequired",
                ServiceFeatureMessages.CustomInputsRequired);
        }

        foreach (var input in customInputs)
        {
            if (input is null)
            {
                return Error(
                    operation,
                    "CustomInputNameRequired",
                    ServiceFeatureMessages.CustomInputNameRequired);
            }

            var normalizedName = input.Name?.Trim();
            if (string.IsNullOrEmpty(normalizedName))
            {
                return Error(
                    operation,
                    "CustomInputNameRequired",
                    ServiceFeatureMessages.CustomInputNameRequired);
            }

            if (normalizedName.Length > 100)
            {
                return Error(
                    operation,
                    "CustomInputNameMaximumLength",
                    ServiceFeatureMessages.CustomInputNameMaximumLength);
            }

            if (NormalizedLength(input.LabelEn) > 200 ||
                NormalizedLength(input.LabelAr) > 200)
            {
                return Error(
                    operation,
                    "CustomInputLabelMaximumLength",
                    ServiceFeatureMessages.CustomInputLabelMaximumLength);
            }

            if (!Enum.IsDefined(input.Type))
            {
                return Error(
                    operation,
                    "InvalidCustomInputType",
                    ServiceFeatureMessages.InvalidCustomInputType);
            }

            if (input.Order <= 0)
            {
                return Error(
                    operation,
                    "InvalidCustomInputOrder",
                    ServiceFeatureMessages.InvalidCustomInputOrder);
            }

            if (input.StartWith is not null &&
                string.IsNullOrWhiteSpace(input.StartWith))
            {
                return Error(
                    operation,
                    "CustomInputStartWithEmpty",
                    ServiceFeatureMessages.CustomInputStartWithEmpty);
            }

            if (NormalizedLength(input.StartWith) > 100)
            {
                return Error(
                    operation,
                    "CustomInputStartWithMaximumLength",
                    ServiceFeatureMessages.CustomInputStartWithMaximumLength);
            }

            var restrictionError = ValidateRestrictions(input, operation);
            if (restrictionError is not null)
            {
                return restrictionError;
            }

            if (validateIds &&
                input is UpdateServiceCustomInputCommand
                {
                    CustomInputId: <= 0
                })
            {
                return Error(
                    operation,
                    "CustomInputNotFound",
                    ServiceFeatureMessages.CustomInputNotFound);
            }
        }

        if (customInputs
            .GroupBy(x => x.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Any(x => x.Count() > 1))
        {
            return Error(
                operation,
                "DuplicateCustomInputName",
                ServiceFeatureMessages.DuplicateCustomInputName);
        }

        if (customInputs.GroupBy(x => x.Order).Any(x => x.Count() > 1))
        {
            return Error(
                operation,
                "DuplicateCustomInputOrder",
                ServiceFeatureMessages.DuplicateCustomInputOrder);
        }

        if (validateIds && customInputs
            .OfType<UpdateServiceCustomInputCommand>()
            .Where(x => x.CustomInputId.HasValue)
            .GroupBy(x => x.CustomInputId!.Value)
            .Any(x => x.Count() > 1))
        {
            return Error(
                operation,
                "DuplicateCustomInputId",
                ServiceFeatureMessages.DuplicateCustomInputId);
        }

        return null;
    }

    private static Error? ValidateRestrictions(
        ServiceCustomInputDefinitionCommand input,
        string operation)
    {
        if (input.Type == ServiceCustomInputType.String)
        {
            if (input.MinValue.HasValue || input.MaxValue.HasValue)
            {
                return Error(
                    operation,
                    "InvalidStringRestrictions",
                    ServiceFeatureMessages.InvalidStringRestrictions);
            }

            if (input.MinLength is < 0 ||
                input.MaxLength is <= 0 or > 3000)
            {
                return Error(
                    operation,
                    "InvalidStringRestrictions",
                    ServiceFeatureMessages.InvalidStringRestrictions);
            }

            if (input.MinLength.HasValue &&
                input.MaxLength.HasValue &&
                input.MaxLength.Value < input.MinLength.Value)
            {
                return Error(
                    operation,
                    "InvalidLengthRange",
                    ServiceFeatureMessages.InvalidLengthRange);
            }

            return null;
        }

        if (input.MinLength.HasValue ||
            input.MaxLength.HasValue ||
            input.StartWith is not null)
        {
            return input.StartWith is not null
                ? Error(
                    operation,
                    "StartWithStringOnly",
                    ServiceFeatureMessages.StartWithStringOnly)
                : Error(
                    operation,
                    "InvalidIntegerRestrictions",
                    ServiceFeatureMessages.InvalidIntegerRestrictions);
        }

        if (input.MinValue.HasValue &&
            input.MaxValue.HasValue &&
            input.MaxValue.Value < input.MinValue.Value)
        {
            return Error(
                operation,
                "InvalidValueRange",
                ServiceFeatureMessages.InvalidValueRange);
        }

        return null;
    }

    private static int NormalizedLength(string? value) =>
        string.IsNullOrWhiteSpace(value) ? 0 : value.Trim().Length;

    private static Error Error(
        string operation,
        string suffix,
        string message) => new(
            $"Services.{operation}.{suffix}",
            message,
            ErrorType.Validation);
}
