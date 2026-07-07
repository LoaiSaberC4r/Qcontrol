using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Services;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Query.GetAvailableParentServices;

internal sealed class GetAvailableParentServicesQueryHandler
    : IQueryHandler<GetAvailableParentServicesQuery, IReadOnlyList<AvailableParentServiceResponse>>
{
    private readonly IWriteReadRepository<Service> _serviceReadRepository;
    private readonly IServiceTicketUsageChecker _ticketUsageChecker;
    private readonly ICurrentUser _currentUser;

    public GetAvailableParentServicesQueryHandler(
        IWriteReadRepository<Service> serviceReadRepository,
        IServiceTicketUsageChecker ticketUsageChecker,
        ICurrentUser currentUser)
    {
        _serviceReadRepository = serviceReadRepository
            ?? throw new ArgumentNullException(nameof(serviceReadRepository));
        _ticketUsageChecker = ticketUsageChecker
            ?? throw new ArgumentNullException(nameof(ticketUsageChecker));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<IReadOnlyList<AvailableParentServiceResponse>>> Handle(
        GetAvailableParentServicesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<AvailableParentServiceResponse>>.Fail(new Error(
                "Services.Authentication.Required",
                ServiceFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var allItems = await _serviceReadRepository.ListAsync(
            new GetAllServiceHierarchyItemsSpec(),
            cancellationToken);
        var states = ServiceHierarchyCalculator.ComputeStates(allItems);
        var excludedDescendants = request.ExcludeServiceId.HasValue
            ? ServiceHierarchyCalculator.GetDescendantIds(
                allItems,
                request.ExcludeServiceId.Value)
            : new HashSet<int>();

        var candidates = allItems
            .Where(x => !x.IsDeleted)
            .Where(x => x.IsActive)
            .Where(x => !x.IsTicketIssuable)
            .Where(x =>
                !request.ExcludeServiceId.HasValue ||
                x.Id != request.ExcludeServiceId.Value)
            .Where(x => !excludedDescendants.Contains(x.Id));

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim();
            candidates = candidates.Where(x =>
                x.ArabicName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                x.EnglishName.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        var responses = new List<AvailableParentServiceResponse>();

        foreach (var candidate in candidates
                     .OrderBy(x => x.ParentServiceId ?? 0)
                     .ThenBy(x => x.OrderNo)
                     .ThenBy(x => x.ArabicName)
                     .ThenBy(x => x.Id))
        {
            var hasHistoricalTickets =
                await _ticketUsageChecker.HasHistoricalTicketsAsync(
                    candidate.Id,
                    cancellationToken);

            if (hasHistoricalTickets)
            {
                continue;
            }

            responses.Add(new AvailableParentServiceResponse
            {
                Id = candidate.Id,
                ParentServiceId = candidate.ParentServiceId,
                ArabicName = candidate.ArabicName,
                EnglishName = candidate.EnglishName,
                EffectiveIsActive = states[candidate.Id].EffectiveIsActive
            });
        }

        return Result<IReadOnlyList<AvailableParentServiceResponse>>.Ok(responses);
    }
}
