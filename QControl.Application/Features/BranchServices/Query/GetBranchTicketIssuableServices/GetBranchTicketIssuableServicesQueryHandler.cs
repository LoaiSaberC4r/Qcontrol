using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;

internal sealed class GetBranchTicketIssuableServicesQueryHandler
    : IQueryHandler<
        GetBranchTicketIssuableServicesQuery,
        GetBranchTicketIssuableServicesResponse>
{
    private const string ErrorCodePrefix =
        "BranchTicketIssuableServices.View";

    private readonly IWriteReadRepository<Branch> _branchReadRepository;

    private readonly IWriteReadRepository<BranchService>
        _branchServiceReadRepository;

    private readonly IWriteReadRepository<Service> _serviceReadRepository;

    private readonly IWriteReadRepository<ServiceSchedule>
        _scheduleReadRepository;

    private readonly IWriteReadRepository<QControl.Domain.Entities.GeneralBrand>
        _generalBrandReadRepository;

    private readonly IWriteReadRepository<Reservation> _reservationReadRepository;

    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetBranchTicketIssuableServicesQueryHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchService> branchServiceReadRepository,
        IWriteReadRepository<Service> serviceReadRepository,
        IWriteReadRepository<ServiceSchedule> scheduleReadRepository,
        IWriteReadRepository<QControl.Domain.Entities.GeneralBrand>
            generalBrandReadRepository,
        IWriteReadRepository<Reservation> reservationReadRepository,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator,
        IDateTimeProvider dateTimeProvider)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _branchServiceReadRepository = branchServiceReadRepository
            ?? throw new ArgumentNullException(
                nameof(branchServiceReadRepository));
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _scheduleReadRepository = scheduleReadRepository
            ?? throw new ArgumentNullException(nameof(scheduleReadRepository));
        _generalBrandReadRepository = generalBrandReadRepository
            ?? throw new ArgumentNullException(
                nameof(generalBrandReadRepository));
        _reservationReadRepository = reservationReadRepository
            ?? throw new ArgumentNullException(nameof(reservationReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAccessValidator = branchAccessValidator
            ?? throw new ArgumentNullException(nameof(branchAccessValidator));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<Result<GetBranchTicketIssuableServicesResponse>> Handle(
        GetBranchTicketIssuableServicesQuery request,
        CancellationToken cancellationToken)
    {
        //if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        //{
        //    return Result<GetBranchTicketIssuableServicesResponse>.Fail(
        //        new Error(
        //            $"{ErrorCodePrefix}.Unauthenticated",
        //            BranchTicketIssuableServicesMessages.AuthenticationRequired,
        //            ErrorType.Unauthorized));
        //}

        //var accessResult = _branchAccessValidator.EnsureCanAccessBranch(
        //    request.BranchId,
        //    ErrorCodePrefix);

        //if (accessResult.IsFailure)
        //{
        //    return Result<GetBranchTicketIssuableServicesResponse>.Fail(
        //        accessResult.Errors);
        //}

        var branchState = await _branchReadRepository.FirstOrDefaultAsync(
            new GetBranchTicketIssuanceStateSpec(request.BranchId),
            cancellationToken);

        if (branchState is null)
        {
            return Result<GetBranchTicketIssuableServicesResponse>.Fail(
                new Error(
                    $"{ErrorCodePrefix}.BranchNotFound",
                    BranchTicketIssuableServicesMessages.BranchNotFound,
                    ErrorType.NotFound));
        }

        if (!branchState.IsActive)
        {
            return Result<GetBranchTicketIssuableServicesResponse>.Fail(
                new Error(
                    $"{ErrorCodePrefix}.BranchInactive",
                    BranchTicketIssuableServicesMessages.BranchInactive,
                    ErrorType.Conflict));
        }

        TicketIssuanceBranchBrandingResponse? branding;
        if (branchState.BrandingId.HasValue)
        {
            branding = TicketIssuanceBrandingResponseFactory.FromLayout(
                branchState,
                BranchMediaUrlMapper.ToMediaUrl(branchState.LogoPath));
        }
        else
        {
            var generalBrand = await _generalBrandReadRepository
                .FirstOrDefaultAsync(
                    new GetGeneralBrandTicketIssuanceStateSpec(),
                    cancellationToken);
            branding = generalBrand is null
                ? null
                : TicketIssuanceBrandingResponseFactory.FromLayout(
                    generalBrand,
                    logoUrl: null);
        }
        var currentDateTime = _dateTimeProvider.UtcNow;
        var currentDay = currentDateTime.DayOfWeek;
        var currentTime = TimeOnly.FromDateTime(currentDateTime);
        var allowedUntil = ServiceScheduleAvailability.CalculateAllowedUntil(
            currentTime,
            branchState.AllowedTime);
        var assignedServiceIds = (await _branchServiceReadRepository.ListAsync(
                new GetAssignedBranchServiceIdsSpec(request.BranchId),
                cancellationToken))
            .ToHashSet();

        if (assignedServiceIds.Count == 0)
        {
            return Success(request.BranchId, branding);
        }

        var availableScheduleServiceIds =
            (await _scheduleReadRepository.ListAsync(
                new GetAllowedScheduledServiceIdsSpec(
                    request.BranchId,
                    currentDay,
                    currentTime,
                    allowedUntil),
                cancellationToken))
            .ToHashSet();

        if (availableScheduleServiceIds.Count == 0)
        {
            return Success(request.BranchId, branding);
        }

        var allItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var states = ServiceHierarchyCalculator.ComputeStates(allItems);
        var eligibleLeafIds = allItems
            .Where(item => assignedServiceIds.Contains(item.Id))
            .Where(item => availableScheduleServiceIds.Contains(item.Id))
            .Where(item => !item.IsDeleted && item.IsActive)
            .Where(item =>
                item.Scope == ServiceScope.Global ||
                (item.Scope == ServiceScope.BranchScoped &&
                 item.OwnerBranchId == request.BranchId))
            .Where(item =>
                states.TryGetValue(item.Id, out var state) &&
                state.CanIssueTicket)
            .Select(item => item.Id)
            .ToHashSet();
        var customInputsByServiceId =
            await LoadCustomInputsAsync(
                eligibleLeafIds,
                cancellationToken);
        var reservationServiceIds = allItems
            .Where(item => item.HasReservation && eligibleLeafIds.Contains(item.Id))
            .Select(item => item.Id)
            .ToArray();
        var historicalInputsByServiceId = await LoadHistoricalSearchInputsAsync(
            request.BranchId, reservationServiceIds, cancellationToken);
        var includedIds = TicketIssuableServiceTreeBuilder.CollectRequiredServiceIds(
            allItems.ToDictionary(x => x.Id), eligibleLeafIds);
        var imagesByServiceId = await LoadImagesAsync(includedIds, cancellationToken);
        var services = TicketIssuableServiceTreeBuilder.Build(
            allItems,
            eligibleLeafIds,
            customInputsByServiceId,
            imagesByServiceId,
            historicalInputsByServiceId);

        return Success(request.BranchId, branding, services);
    }

    private async Task<IReadOnlyDictionary<int, ServiceImagesForResponse>> LoadImagesAsync(
        IReadOnlySet<int> includedIds, CancellationToken cancellationToken)
    {
        if (includedIds.Count == 0)
            return new Dictionary<int, ServiceImagesForResponse>();

        var ids = includedIds.ToArray();
        var images = await _serviceReadRepository.Query().AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                LogoPath = x.Images.Where(image => image.IsActive &&
                        image.ImageType == ServiceImageType.Logo)
                    .OrderBy(image => image.DisplayOrder).ThenBy(image => image.Id)
                    .Select(image => image.ImagePath).FirstOrDefault(),
                IconPath = x.Images.Where(image => image.IsActive &&
                        image.ImageType == ServiceImageType.Icon)
                    .OrderBy(image => image.DisplayOrder).ThenBy(image => image.Id)
                    .Select(image => image.ImagePath).FirstOrDefault()
            }).ToListAsync(cancellationToken);

        return images.ToDictionary(x => x.Id, x => new ServiceImagesForResponse
        {
            LogoUrl = ServiceMediaUrlMapper.ToMediaUrl(x.LogoPath),
            IconUrl = ServiceMediaUrlMapper.ToMediaUrl(x.IconPath)
        });
    }

    private async Task<IReadOnlyDictionary<
        int,
        IReadOnlyList<ServiceCustomInputResponse>>> LoadCustomInputsAsync(
        IReadOnlySet<int> eligibleLeafIds,
        CancellationToken cancellationToken)
    {
        if (eligibleLeafIds.Count == 0)
        {
            return new Dictionary<int, IReadOnlyList<ServiceCustomInputResponse>>();
        }

        var ids = eligibleLeafIds.ToArray();
        var items = await _serviceReadRepository.Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.IsClientInputRequired)
            .SelectMany(x => x.CustomInputs)
            .Where(x => x.IsActive)
            .OrderBy(x => x.ServiceId)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Id)
            .Select(x => new ServiceCustomInputReadModel
            {
                ServiceId = x.ServiceId,
                CustomInputId = x.Id,
                Name = x.Name,
                LabelEn = x.LabelEn,
                LabelAr = x.LabelAr,
                Type = x.Type,
                IsRequired = x.IsRequired,
                MinLength = x.MinLength,
                MaxLength = x.MaxLength,
                MinValue = x.MinValue,
                MaxValue = x.MaxValue,
                StartWith = x.StartWith,
                Order = x.Order
            })
            .ToListAsync(cancellationToken);

        return items
            .GroupBy(x => x.ServiceId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<ServiceCustomInputResponse>)group
                    .Select(ServiceCustomInputResponseFactory.FromReadModel)
                    .ToList());
    }

    private async Task<IReadOnlyDictionary<int, IReadOnlyList<ServiceCustomInputResponse>>>
        LoadHistoricalSearchInputsAsync(
            int branchId, int[] serviceIds, CancellationToken cancellationToken)
    {
        if (serviceIds.Length == 0)
            return new Dictionary<int, IReadOnlyList<ServiceCustomInputResponse>>();

        var latestSnapshots = await _reservationReadRepository.Query().AsNoTracking()
            .Where(reservation => reservation.BranchId == branchId &&
                serviceIds.Contains(reservation.ServiceId))
            .SelectMany(reservation => reservation.CustomInputValues
                .Where(snapshot => snapshot.OriginalServiceCustomInputId.HasValue)
                .Select(snapshot => new
                {
                    reservation.ServiceId,
                    OriginalId = snapshot.OriginalServiceCustomInputId!.Value,
                    snapshot.Id,
                    snapshot.CreatedOnUtc,
                    snapshot.NameSnapshot,
                    snapshot.LabelEnSnapshot,
                    snapshot.LabelArSnapshot,
                    snapshot.TypeSnapshot,
                    snapshot.OrderSnapshot
                }))
            .GroupBy(snapshot => new { snapshot.ServiceId, snapshot.OriginalId })
            .Select(group => group.OrderByDescending(snapshot => snapshot.CreatedOnUtc)
                .ThenByDescending(snapshot => snapshot.Id).First())
            .ToListAsync(cancellationToken);

        return latestSnapshots.GroupBy(snapshot => snapshot.ServiceId).ToDictionary(
            group => group.Key,
            group => (IReadOnlyList<ServiceCustomInputResponse>)group
                .OrderBy(snapshot => snapshot.OrderSnapshot ?? int.MaxValue)
                .ThenBy(snapshot => snapshot.OriginalId)
                .Select(snapshot => new ServiceCustomInputResponse
                {
                    CustomInputId = snapshot.OriginalId,
                    Name = snapshot.NameSnapshot,
                    LabelEn = snapshot.LabelEnSnapshot,
                    LabelAr = snapshot.LabelArSnapshot,
                    Type = snapshot.TypeSnapshot,
                    TypeName = snapshot.TypeSnapshot.ToString(),
                    Order = snapshot.OrderSnapshot ?? int.MaxValue
                }).ToArray());
    }

    private static Result<GetBranchTicketIssuableServicesResponse> Success(
        int branchId,
        TicketIssuanceBranchBrandingResponse? branding,
        IReadOnlyList<TicketIssuableServiceTreeNodeResponse>? services = null) =>
        Result<GetBranchTicketIssuableServicesResponse>.Ok(new()
        {
            BranchId = branchId,
            BranchBranding = branding,
            Services = services ??
                Array.Empty<TicketIssuableServiceTreeNodeResponse>()
        });
}
