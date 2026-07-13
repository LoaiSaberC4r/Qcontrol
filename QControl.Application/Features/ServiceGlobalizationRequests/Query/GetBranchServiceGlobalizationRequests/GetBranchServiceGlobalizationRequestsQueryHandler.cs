using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Query.GetServiceGlobalizationRequests;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Query.GetBranchServiceGlobalizationRequests;

internal sealed class GetBranchServiceGlobalizationRequestsQueryHandler
    : IQueryHandler<
        GetBranchServiceGlobalizationRequestsQuery,
        Pagination<ServiceGlobalizationRequestListItemResponse>>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<ServiceGlobalizationRequest>
        _requestReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBranchAccessValidator _branchAccessValidator;

    public GetBranchServiceGlobalizationRequestsQueryHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<ServiceGlobalizationRequest> requestReadRepository,
        ICurrentUser currentUser,
        IBranchAccessValidator branchAccessValidator)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _requestReadRepository = requestReadRepository
            ?? throw new ArgumentNullException(nameof(requestReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAccessValidator = branchAccessValidator
            ?? throw new ArgumentNullException(nameof(branchAccessValidator));
    }

    public async Task<Result<Pagination<ServiceGlobalizationRequestListItemResponse>>>
        Handle(
            GetBranchServiceGlobalizationRequestsQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "ServiceGlobalizationRequests.GetBranch.Unauthenticated",
                ServiceGlobalizationRequestMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        var branchAccess = _branchAccessValidator.EnsureCanAccessBranch(
            request.BranchId,
            "ServiceGlobalizationRequests.GetBranch");

        if (branchAccess.IsFailure)
        {
            return Result<Pagination<ServiceGlobalizationRequestListItemResponse>>
                .Fail(branchAccess.Errors);
        }

        var branchExists = await _branchReadRepository.Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == request.BranchId, cancellationToken);

        if (!branchExists)
        {
            return Failure(
                "ServiceGlobalizationRequests.GetBranch.BranchNotFound",
                ServiceFeatureMessages.BranchNotFound,
                ErrorType.NotFound);
        }

        request.SearchText ??= string.Empty;

        var query =
            GetServiceGlobalizationRequestsQueryHandler.BuildFilteredQuery(
                _requestReadRepository.Query().AsNoTracking(),
                request.Status,
                request.RequestType,
                request.BranchId,
                request.SearchText);

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.OrderSort == OrderSort.Oldest
            ? query
                .OrderBy(x => x.RequestedOnUtc)
                .ThenBy(x => x.Id)
            : query
                .OrderByDescending(x => x.RequestedOnUtc)
                .ThenByDescending(x => x.Id);

        var items = await GetServiceGlobalizationRequestsQueryHandler
            .Project(query)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return Result<Pagination<ServiceGlobalizationRequestListItemResponse>>
            .Ok(new Pagination<ServiceGlobalizationRequestListItemResponse>(
                request.PageNumber,
                request.PageSize,
                totalCount,
                items
                    .Select(ServiceGlobalizationRequestResponseFactory
                        .ToListItem)
                    .ToList()));
    }

    private static Result<Pagination<ServiceGlobalizationRequestListItemResponse>>
        Failure(
            string code,
            string message,
            ErrorType type)
        => Result<Pagination<ServiceGlobalizationRequestListItemResponse>>.Fail(
            new Error(code, message, type));
}
