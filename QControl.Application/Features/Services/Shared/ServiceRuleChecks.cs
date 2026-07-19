using BuildingBlock.Domain.Results;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Services;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Services.Shared;

internal static class ServiceRuleChecks
{
    public static async Task<Error?> ValidateParentAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        IServiceTicketUsageChecker ticketUsageChecker,
        int parentServiceId,
        int? currentServiceId,
        ServiceScope expectedScope,
        int? expectedOwnerBranchId,
        string operation,
        CancellationToken cancellationToken)
    {
        if (currentServiceId.HasValue &&
            currentServiceId.Value == parentServiceId)
        {
            return new Error(
                $"Services.{operation}.CircularHierarchy",
                ServiceFeatureMessages.CircularHierarchy,
                ErrorType.Conflict);
        }

        var parent = await serviceReadRepository.FirstOrDefaultAsync(
            new GetServiceHierarchyItemByIdSpec(
                parentServiceId,
                includeDeleted: true),
            cancellationToken);

        if (parent is null)
        {
            return new Error(
                $"Services.{operation}.ParentNotFound",
                ServiceFeatureMessages.ParentNotFound,
                ErrorType.NotFound);
        }

        if (parent.IsDeleted)
        {
            return new Error(
                $"Services.{operation}.ParentDeleted",
                ServiceFeatureMessages.ParentDeleted,
                ErrorType.Conflict);
        }

        if (!parent.IsActive)
        {
            return new Error(
                $"Services.{operation}.ParentInactive",
                ServiceFeatureMessages.ParentInactive,
                ErrorType.Conflict);
        }

        if (parent.IsTicketIssuable)
        {
            return new Error(
                $"Services.{operation}.ParentTicketIssuable",
                ServiceFeatureMessages.ParentTicketIssuable,
                ErrorType.Conflict);
        }

        if (parent.Scope != expectedScope)
        {
            return new Error(
                $"Services.{operation}.ParentScopeMismatch",
                ServiceFeatureMessages.ParentScopeMismatch,
                ErrorType.Conflict);
        }

        if (parent.OwnerBranchId != expectedOwnerBranchId)
        {
            return new Error(
                $"Services.{operation}.ParentOwnerMismatch",
                ServiceFeatureMessages.ParentOwnerMismatch,
                ErrorType.Conflict);
        }

        if (currentServiceId.HasValue)
        {
            var allItems = await serviceReadRepository.ListAsync(
                new GetAllServiceHierarchyItemsSpec(),
                cancellationToken);

            var descendantIds = ServiceHierarchyCalculator.GetDescendantIds(
                allItems,
                currentServiceId.Value);

            if (descendantIds.Contains(parentServiceId))
            {
                return new Error(
                    $"Services.{operation}.CircularHierarchy",
                    ServiceFeatureMessages.CircularHierarchy,
                    ErrorType.Conflict);
            }
        }

        var parentHasHistoricalTickets =
            await ticketUsageChecker.HasHistoricalTicketsAsync(
                parentServiceId,
                cancellationToken);

        if (parentHasHistoricalTickets)
        {
            return new Error(
                $"Services.{operation}.ParentHasHistoricalTickets",
                ServiceFeatureMessages.ParentHasHistoricalTickets,
                ErrorType.Conflict);
        }

        return null;
    }

    public static async Task<Error?> ValidateDuplicateNamesAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        string arabicName,
        string englishName,
        int? parentServiceId,
        ServiceScope scope,
        int? ownerBranchId,
        int? excludedServiceId,
        string operation,
        CancellationToken cancellationToken)
    {
        var duplicateArabicId =
            await serviceReadRepository.FirstOrDefaultAsync(
                new ServiceDuplicateArabicNameSpec(
                    arabicName,
                    parentServiceId,
                    scope,
                    ownerBranchId,
                    excludedServiceId),
                cancellationToken);

        if (duplicateArabicId > 0)
        {
            return new Error(
                $"Services.{operation}.DuplicateArabicName",
                ServiceFeatureMessages.DuplicateArabicName,
                ErrorType.Conflict);
        }

        var duplicateEnglishId =
            await serviceReadRepository.FirstOrDefaultAsync(
                new ServiceDuplicateEnglishNameSpec(
                    englishName,
                    parentServiceId,
                    scope,
                    ownerBranchId,
                    excludedServiceId),
                cancellationToken);

        if (duplicateEnglishId > 0)
        {
            return new Error(
                $"Services.{operation}.DuplicateEnglishName",
                ServiceFeatureMessages.DuplicateEnglishName,
                ErrorType.Conflict);
        }

        return null;
    }

    public static async Task<Error?> ValidateServiceCodeIsUniqueAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        string? normalizedServiceCode,
        int? excludedServiceId,
        string errorCode,
        CancellationToken cancellationToken)
    {
        if (normalizedServiceCode is null)
        {
            return null;
        }

        var duplicateServiceId =
            await serviceReadRepository.FirstOrDefaultAsync(
                new ServiceDuplicateCodeSpec(
                    normalizedServiceCode,
                    excludedServiceId),
                cancellationToken);

        return duplicateServiceId > 0
            ? new Error(
                errorCode,
                ServiceFeatureMessages.ServiceCodeAlreadyExists,
                ErrorType.Conflict)
            : null;
    }

    public static async Task<Error?> ValidateServiceCodesAreUniqueAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        IEnumerable<string?> serviceCodes,
        string errorCode,
        CancellationToken cancellationToken)
    {
        var normalizedCodes = serviceCodes
            .Select(ServiceCodeNormalizer.Normalize)
            .Where(x => x is not null)
            .Select(x => x!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedCodes.Length == 0)
        {
            return null;
        }

        var conflicts = await serviceReadRepository.ListAsync(
            new ServiceCodesInUseSpec(normalizedCodes),
            cancellationToken);

        return conflicts.Count > 0
            ? new Error(
                errorCode,
                ServiceFeatureMessages.ServiceCodeAlreadyExists,
                ErrorType.Conflict)
            : null;
    }

    public static Error? ValidateTicketSettings(
        bool isTicketIssuable,
        string? rangePrefix,
        int? rangeStartNumber,
        int? rangeEndNumber,
        int? waitingDuration,
        int? noOfTicketCopies,
        string operation)
    {
        if (!isTicketIssuable)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(rangePrefix) ||
            !rangeStartNumber.HasValue ||
            !rangeEndNumber.HasValue ||
            !waitingDuration.HasValue ||
            !noOfTicketCopies.HasValue)
        {
            return new Error(
                $"Services.{operation}.TicketSettingsRequired",
                ServiceFeatureMessages.TicketSettingsRequired,
                ErrorType.Validation);
        }

        return null;
    }

    public static async Task<ServiceResponse?> BuildServiceResponseAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        int serviceId,
        ServiceResponseAccessContext accessContext,
        string? message,
        CancellationToken cancellationToken)
    {
        var items = await serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);

        var item = items.FirstOrDefault(x => x.Id == serviceId);
        if (item is null)
        {
            return null;
        }

        var states = ServiceHierarchyCalculator.ComputeStates(items);

        return ServiceResponseFactory.FromItem(
            item,
            states[item.Id],
            accessContext,
            ServiceImagesForResponse.Empty,
            message);
    }

    public static async Task<ServiceRestoreResponse?> BuildRestoreResponseAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        int serviceId,
        string? message,
        CancellationToken cancellationToken)
    {
        var items = await serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);

        var item = items.FirstOrDefault(x => x.Id == serviceId);
        if (item is null)
        {
            return null;
        }

        var states = ServiceHierarchyCalculator.ComputeStates(items);

        return ServiceResponseFactory.Restored(
            item,
            states[item.Id],
            message);
    }
}
