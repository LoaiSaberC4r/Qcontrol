using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Query.GetSegmentGlobalizationRequests;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Query.GetBranchSegmentGlobalizationRequests;

internal sealed class GetBranchSegmentGlobalizationRequestsQueryHandler
    : IQueryHandler<
        GetBranchSegmentGlobalizationRequestsQuery,
        Pagination<SegmentGlobalizationRequestResponse>>
{
    private readonly IWriteReadRepository<SegmentGlobalizationRequest>
        _requests;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _branchContext;

    public GetBranchSegmentGlobalizationRequestsQueryHandler(
        IWriteReadRepository<SegmentGlobalizationRequest> requests,
        ICurrentUser currentUser,
        ICurrentBranchContext branchContext)
    {
        _requests = requests;
        _currentUser = currentUser;
        _branchContext = branchContext;
    }

    public async Task<Result<Pagination<SegmentGlobalizationRequestResponse>>>
        Handle(
            GetBranchSegmentGlobalizationRequestsQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "SegmentGlobalizationRequests.AuthenticationRequired",
                SegmentGlobalizationRequestMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        if (!_branchContext.IsBranchActor ||
            _branchContext.ActiveBranchId != request.BranchId)
        {
            return Failure(
                "SegmentGlobalizationRequests.BranchAccessForbidden",
                SegmentGlobalizationRequestMessages.BranchAccessForbidden,
                ErrorType.Security);
        }

        return await GetSegmentGlobalizationRequestsQueryHandler.LoadAsync(
            _requests,
            request.Status,
            request.BranchId,
            request.SearchText,
            request.PageNumber,
            request.PageSize,
            request.OrderSort,
            cancellationToken);
    }

    private static Result<Pagination<SegmentGlobalizationRequestResponse>>
        Failure(
            string code,
            string message,
            ErrorType type) =>
            Result<Pagination<SegmentGlobalizationRequestResponse>>.Fail(
                new Error(code, message, type));
}
