using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceTree;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchServiceTrees.Shared;

internal sealed class BranchServiceTreePayloadValidationOptions
{
    public string CodePrefix { get; init; } = string.Empty;

    public int MaxNodeCount { get; init; } = 1000;

    public string MaximumNodesExceededCode { get; init; } = string.Empty;

    public string MaximumNodesExceededMessage { get; init; } = string.Empty;

    public string TicketIssuableCannotHaveChildrenMessage { get; init; } =
        string.Empty;

    public string DuplicateArabicNameMessage { get; init; } = string.Empty;

    public string DuplicateEnglishNameMessage { get; init; } = string.Empty;

    public bool UseCodePrefixForTicketSettings { get; init; }
}

internal static class BranchServiceTreePayloadValidator
{
    public static Error? Validate(
        CreateBranchServiceTreeNodeCommand root,
        BranchServiceTreePayloadValidationOptions options,
        ICollection<string>? normalizedServiceCodes = null)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(options);

        var total = 0;
        var serviceCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stack = new Stack<CreateBranchServiceTreeNodeCommand>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            total++;

            if (total > options.MaxNodeCount)
            {
                return new Error(
                    options.MaximumNodesExceededCode,
                    options.MaximumNodesExceededMessage,
                    ErrorType.Validation);
            }

            var nodeValidation = ValidateNodeFields(node, options);
            if (nodeValidation is not null)
            {
                return nodeValidation;
            }

            var normalizedServiceCode =
                ServiceCodeNormalizer.Normalize(node.ServiceCode);
            if (normalizedServiceCode is not null &&
                !serviceCodes.Add(normalizedServiceCode))
            {
                return new Error(
                    $"{options.CodePrefix}.DuplicateServiceCodeInTreePayload",
                    ServiceFeatureMessages.DuplicateServiceCodeInTreePayload,
                    ErrorType.Validation);
            }

            if (normalizedServiceCode is not null)
            {
                normalizedServiceCodes?.Add(normalizedServiceCode);
            }

            var children = node.Children ??
                Array.Empty<CreateBranchServiceTreeNodeCommand>();

            if (children.Count > 0 &&
                node.IsTicketIssuable.GetValueOrDefault())
            {
                return new Error(
                    $"{options.CodePrefix}.TicketIssuableCannotHaveChildren",
                    options.TicketIssuableCannotHaveChildrenMessage,
                    ErrorType.Conflict);
            }

            var duplicateSibling = ValidateSiblingNames(children, options);
            if (duplicateSibling is not null)
            {
                return duplicateSibling;
            }

            foreach (var child in children)
            {
                stack.Push(child);
            }
        }

        return null;
    }

    private static Error? ValidateNodeFields(
        CreateBranchServiceTreeNodeCommand node,
        BranchServiceTreePayloadValidationOptions options)
    {
        if (string.IsNullOrWhiteSpace(node.ArabicName))
        {
            return new Error(
                $"{options.CodePrefix}.ArabicNameRequired",
                ServiceFeatureMessages.ArabicNameRequired,
                ErrorType.Validation);
        }

        if (node.ArabicName.Length > 100)
        {
            return new Error(
                $"{options.CodePrefix}.ArabicNameMaxLength",
                ServiceFeatureMessages.ArabicNameMaxLength,
                ErrorType.Validation);
        }

        if (string.IsNullOrWhiteSpace(node.EnglishName))
        {
            return new Error(
                $"{options.CodePrefix}.EnglishNameRequired",
                ServiceFeatureMessages.EnglishNameRequired,
                ErrorType.Validation);
        }

        if (node.EnglishName.Length > 100)
        {
            return new Error(
                $"{options.CodePrefix}.EnglishNameMaxLength",
                ServiceFeatureMessages.EnglishNameMaxLength,
                ErrorType.Validation);
        }

        var normalizedServiceCode =
            ServiceCodeNormalizer.Normalize(node.ServiceCode);

        if (node.IsServiceCodeRequired && normalizedServiceCode is null)
        {
            return new Error(
                $"{options.CodePrefix}.ServiceCodeRequired",
                ServiceFeatureMessages.ServiceCodeRequired,
                ErrorType.Validation);
        }

        if (!node.IsServiceCodeRequired && normalizedServiceCode is not null)
        {
            return new Error(
                $"{options.CodePrefix}.ServiceCodeMustBeNull",
                ServiceFeatureMessages.ServiceCodeMustBeNull,
                ErrorType.Validation);
        }

        if (normalizedServiceCode?.Length > ServiceCodeNormalizer.MaxLength)
        {
            return new Error(
                $"{options.CodePrefix}.ServiceCodeMaximumLength",
                ServiceFeatureMessages.ServiceCodeMaximumLength,
                ErrorType.Validation);
        }

        if (!string.IsNullOrWhiteSpace(node.ArabicUserMessage) &&
            node.ArabicUserMessage.Length > 500)
        {
            return new Error(
                $"{options.CodePrefix}.ArabicUserMessageMaxLength",
                ServiceFeatureMessages.ArabicUserMessageMaxLength,
                ErrorType.Validation);
        }

        if (!string.IsNullOrWhiteSpace(node.EnglishUserMessage) &&
            node.EnglishUserMessage.Length > 500)
        {
            return new Error(
                $"{options.CodePrefix}.EnglishUserMessageMaxLength",
                ServiceFeatureMessages.EnglishUserMessageMaxLength,
                ErrorType.Validation);
        }

        if (!node.IsTicketIssuable.HasValue)
        {
            return new Error(
                $"{options.CodePrefix}.IsTicketIssuableRequired",
                ServiceFeatureMessages.IsTicketIssuableRequired,
                ErrorType.Validation);
        }

        if (node.OrderNo < 0)
        {
            return new Error(
                $"{options.CodePrefix}.OrderNoNonNegative",
                ServiceFeatureMessages.OrderNoNonNegative,
                ErrorType.Validation);
        }

        if (node.Priority < 0)
        {
            return new Error(
                $"{options.CodePrefix}.PriorityNonNegative",
                ServiceFeatureMessages.PriorityNonNegative,
                ErrorType.Validation);
        }

        if (!string.IsNullOrWhiteSpace(node.RangePrefix) &&
            node.RangePrefix.Length > 10)
        {
            return new Error(
                $"{options.CodePrefix}.RangePrefixMaxLength",
                ServiceFeatureMessages.RangePrefixMaxLength,
                ErrorType.Validation);
        }

        if (node.RangeStartNumber is < 0)
        {
            return new Error(
                $"{options.CodePrefix}.RangeStartNonNegative",
                ServiceFeatureMessages.RangeStartNonNegative,
                ErrorType.Validation);
        }

        if (node.RangeEndNumber.HasValue &&
            node.RangeStartNumber.HasValue &&
            node.RangeEndNumber.Value < node.RangeStartNumber.Value)
        {
            return new Error(
                $"{options.CodePrefix}.RangeEndGreaterOrEqualStart",
                ServiceFeatureMessages.RangeEndGreaterOrEqualStart,
                ErrorType.Validation);
        }

        if (node.WaitingDuration is < 0)
        {
            return new Error(
                $"{options.CodePrefix}.WaitingDurationNonNegative",
                ServiceFeatureMessages.WaitingDurationNonNegative,
                ErrorType.Validation);
        }

        if (node.NoOfTicketCopies is <= 0)
        {
            return new Error(
                $"{options.CodePrefix}.NoOfTicketCopiesPositive",
                ServiceFeatureMessages.NoOfTicketCopiesPositive,
                ErrorType.Validation);
        }

        return ValidateTicketSettings(node, options);
    }

    private static Error? ValidateTicketSettings(
        CreateBranchServiceTreeNodeCommand node,
        BranchServiceTreePayloadValidationOptions options)
    {
        var isTicketIssuable = node.IsTicketIssuable.GetValueOrDefault();

        if (!options.UseCodePrefixForTicketSettings)
        {
            return ServiceRuleChecks.ValidateTicketSettings(
                isTicketIssuable,
                node.RangePrefix,
                node.RangeStartNumber,
                node.RangeEndNumber,
                node.WaitingDuration,
                node.NoOfTicketCopies,
                operation: "Create");
        }

        if (!isTicketIssuable)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(node.RangePrefix) ||
            !node.RangeStartNumber.HasValue ||
            !node.RangeEndNumber.HasValue ||
            !node.WaitingDuration.HasValue ||
            !node.NoOfTicketCopies.HasValue)
        {
            return new Error(
                $"{options.CodePrefix}.TicketSettingsRequired",
                ServiceFeatureMessages.TicketSettingsRequired,
                ErrorType.Validation);
        }

        return null;
    }

    private static Error? ValidateSiblingNames(
        IReadOnlyCollection<CreateBranchServiceTreeNodeCommand> siblings,
        BranchServiceTreePayloadValidationOptions options)
    {
        var arabicNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var englishNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var sibling in siblings)
        {
            if (!arabicNames.Add(sibling.ArabicName.Trim()))
            {
                return new Error(
                    $"{options.CodePrefix}.DuplicateArabicName",
                    options.DuplicateArabicNameMessage,
                    ErrorType.Conflict);
            }

            if (!englishNames.Add(sibling.EnglishName.Trim()))
            {
                return new Error(
                    $"{options.CodePrefix}.DuplicateEnglishName",
                    options.DuplicateEnglishNameMessage,
                    ErrorType.Conflict);
            }
        }

        return null;
    }
}
