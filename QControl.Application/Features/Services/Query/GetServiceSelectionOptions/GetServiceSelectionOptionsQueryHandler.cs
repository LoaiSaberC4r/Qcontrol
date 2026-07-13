using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Query.GetServiceSelectionOptions;

internal sealed class GetServiceSelectionOptionsQueryHandler
    : IQueryHandler<GetServiceSelectionOptionsQuery, GetServiceSelectionOptionsResponse>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IServiceVisibilityPolicy _visibilityPolicy;

    public GetServiceSelectionOptionsQueryHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        ICurrentUser currentUser,
        IServiceVisibilityPolicy visibilityPolicy)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _visibilityPolicy = visibilityPolicy
            ?? throw new ArgumentNullException(nameof(visibilityPolicy));
    }

    public async Task<Result<GetServiceSelectionOptionsResponse>> Handle(
        GetServiceSelectionOptionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<GetServiceSelectionOptionsResponse>.Fail(new Error(
                "Services.SelectionOptions.Unauthenticated",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var visibilityContext = _visibilityPolicy.EnsureCanUseVisibilityContext(
            "Services.SelectionOptions");
        if (visibilityContext.IsFailure)
        {
            return Result<GetServiceSelectionOptionsResponse>.Fail(
                visibilityContext.Errors);
        }

        var items = request.ServiceId.HasValue
            ? await LoadChildrenWithParentValidationAsync(
                request.ServiceId.Value,
                cancellationToken)
            : await LoadRootItemsAsync(cancellationToken);

        if (items is null)
        {
            return Result<GetServiceSelectionOptionsResponse>.Fail(new Error(
                "Services.SelectionOptions.ParentNotFound",
                ServiceFeatureMessages.ParentNotFound,
                ErrorType.NotFound));
        }

        var itemIds = items
            .Select(x => x.Id)
            .ToArray();

        var parentIdsThatHaveChildren = itemIds.Length == 0
            ? new List<int>()
            : await _visibilityPolicy.ApplyVisibleServices(
                    _serviceReadRepository.Query())
                .Where(x => !x.IsDeleted)
                .Where(x => x.IsActive)
                .Select(x => x.ParentServiceId)
                .Where(parentServiceId => parentServiceId.HasValue)
                .Select(parentServiceId => parentServiceId.GetValueOrDefault())
                .Where(parentServiceId => itemIds.Contains(parentServiceId))
                .Distinct()
                .ToListAsync(cancellationToken);

        var hasChildrenSet = parentIdsThatHaveChildren.ToHashSet();
        var responseItems = items
            .Select(x =>
            {
                var hasChildren = hasChildrenSet.Contains(x.Id);

                return new ServiceSelectionOptionResponse
                {
                    ServiceId = x.Id,
                    ParentServiceId = x.ParentServiceId,
                    ArabicName = x.ArabicName,
                    EnglishName = x.EnglishName,
                    ArabicUserMessage = x.ArabicUserMessage,
                    EnglishUserMessage = x.EnglishUserMessage,
                    OrderNo = x.OrderNo,
                    Priority = x.Priority,
                    IsActive = x.IsActive,
                    EffectiveIsActive = x.IsActive,
                    IsTicketIssuable = x.IsTicketIssuable,
                    HasChildren = hasChildren,
                    CanIssueTicket = x.IsActive && x.IsTicketIssuable && !hasChildren,
                    HasReservation = x.HasReservation,
                    IsClientInputRequired = x.IsClientInputRequired,
                    RangePrefix = x.RangePrefix,
                    RangeStartNumber = x.RangeStartNumber,
                    RangeEndNumber = x.RangeEndNumber,
                    WaitingDuration = x.WaitingDuration,
                    NoOfTicketCopies = x.NoOfTicketCopies
                };
            })
            .ToList();

        return Result<GetServiceSelectionOptionsResponse>.Ok(
            new GetServiceSelectionOptionsResponse
            {
                ParentServiceId = request.ServiceId,
                Items = responseItems
            });
    }

    private async Task<IReadOnlyList<ServiceSelectionOptionItem>>
        LoadRootItemsAsync(CancellationToken cancellationToken)
    {
        return await _visibilityPolicy.ApplyVisibleServices(
                _serviceReadRepository.Query())
            .Where(x => !x.IsDeleted)
            .Where(x => x.IsActive)
            .Where(x => x.ParentServiceId == null)
            .OrderBy(x => x.OrderNo)
            .ThenBy(x => x.ArabicName)
            .ThenBy(x => x.EnglishName)
            .ThenBy(x => x.Id)
            .Select(x => new ServiceSelectionOptionItem
            {
                Id = x.Id,
                ParentServiceId = x.ParentServiceId,
                ArabicName = x.ArabicName,
                EnglishName = x.EnglishName,
                ArabicUserMessage = x.ArabicUserMessage,
                EnglishUserMessage = x.EnglishUserMessage,
                OrderNo = x.OrderNo,
                Priority = x.Priority,
                IsActive = x.IsActive,
                IsTicketIssuable = x.IsTicketIssuable,
                HasReservation = x.HasReservation,
                IsClientInputRequired = x.IsClientInputRequired,
                RangePrefix = x.RangePrefix,
                RangeStartNumber = x.RangeStartNumber,
                RangeEndNumber = x.RangeEndNumber,
                WaitingDuration = x.WaitingDuration,
                NoOfTicketCopies = x.NoOfTicketCopies
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ServiceSelectionOptionItem>?>
        LoadChildrenWithParentValidationAsync(
            int parentServiceId,
            CancellationToken cancellationToken)
    {
        var rows = await _visibilityPolicy.ApplyVisibleServices(
                _serviceReadRepository.Query())
            .Where(x =>
                (x.Id == parentServiceId && !x.IsDeleted) ||
                (x.ParentServiceId == parentServiceId && !x.IsDeleted && x.IsActive))
            .Select(x => new ServiceSelectionOptionQueryRow
            {
                IsRequestedParent = x.Id == parentServiceId,
                Item = new ServiceSelectionOptionItem
                {
                    Id = x.Id,
                    ParentServiceId = x.ParentServiceId,
                    ArabicName = x.ArabicName,
                    EnglishName = x.EnglishName,
                    ArabicUserMessage = x.ArabicUserMessage,
                    EnglishUserMessage = x.EnglishUserMessage,
                    OrderNo = x.OrderNo,
                    Priority = x.Priority,
                    IsActive = x.IsActive,
                    IsTicketIssuable = x.IsTicketIssuable,
                    HasReservation = x.HasReservation,
                    IsClientInputRequired = x.IsClientInputRequired,
                    RangePrefix = x.RangePrefix,
                    RangeStartNumber = x.RangeStartNumber,
                    RangeEndNumber = x.RangeEndNumber,
                    WaitingDuration = x.WaitingDuration,
                    NoOfTicketCopies = x.NoOfTicketCopies
                }
            })
            .ToListAsync(cancellationToken);

        if (!rows.Any(x => x.IsRequestedParent))
        {
            return null;
        }

        return rows
            .Where(x => !x.IsRequestedParent)
            .Select(x => x.Item)
            .OrderBy(x => x.OrderNo)
            .ThenBy(x => x.ArabicName)
            .ThenBy(x => x.EnglishName)
            .ThenBy(x => x.Id)
            .ToList();
    }

    private sealed class ServiceSelectionOptionQueryRow
    {
        public bool IsRequestedParent { get; init; }

        public ServiceSelectionOptionItem Item { get; init; } = new();
    }

    private sealed class ServiceSelectionOptionItem
    {
        public int Id { get; init; }

        public int? ParentServiceId { get; init; }

        public string ArabicName { get; init; } = string.Empty;

        public string EnglishName { get; init; } = string.Empty;

        public string? ArabicUserMessage { get; init; }

        public string? EnglishUserMessage { get; init; }

        public int OrderNo { get; init; }

        public int Priority { get; init; }

        public bool IsActive { get; init; }

        public bool IsTicketIssuable { get; init; }

        public bool HasReservation { get; init; }

        public bool IsClientInputRequired { get; init; }

        public string? RangePrefix { get; init; }

        public int? RangeStartNumber { get; init; }

        public int? RangeEndNumber { get; init; }

        public int? WaitingDuration { get; init; }

        public int? NoOfTicketCopies { get; init; }
    }
}
