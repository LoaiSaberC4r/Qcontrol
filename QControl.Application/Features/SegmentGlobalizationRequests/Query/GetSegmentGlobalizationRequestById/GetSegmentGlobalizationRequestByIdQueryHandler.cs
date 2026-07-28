using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Query.GetSegmentGlobalizationRequestById;

internal sealed class GetSegmentGlobalizationRequestByIdQueryHandler
    : IQueryHandler<
        GetSegmentGlobalizationRequestByIdQuery,
        SegmentGlobalizationRequestResponse>
{
    private readonly IWriteReadRepository<SegmentGlobalizationRequest>
        _requests;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _branchContext;

    public GetSegmentGlobalizationRequestByIdQueryHandler(
        IWriteReadRepository<SegmentGlobalizationRequest> requests,
        ICurrentUser currentUser,
        ICurrentBranchContext branchContext)
    {
        _requests = requests;
        _currentUser = currentUser;
        _branchContext = branchContext;
    }

    public async Task<Result<SegmentGlobalizationRequestResponse>> Handle(
        GetSegmentGlobalizationRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "SegmentGlobalizationRequests.AuthenticationRequired",
                SegmentGlobalizationRequestMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        var response =
            await SegmentGlobalizationRequestResponseLoader.LoadAsync(
                request.RequestId,
                _requests,
                cancellationToken);
        if (response is null)
        {
            return Failure(
                "SegmentGlobalizationRequests.NotFound",
                SegmentGlobalizationRequestMessages.NotFound,
                ErrorType.NotFound);
        }

        if (!_branchContext.IsSystemLevelActor &&
            (!_branchContext.IsBranchActor ||
             _branchContext.ActiveBranchId != response.BranchId))
        {
            return Failure(
                "SegmentGlobalizationRequests.BranchAccessForbidden",
                SegmentGlobalizationRequestMessages.BranchAccessForbidden,
                ErrorType.Security);
        }

        return Result<SegmentGlobalizationRequestResponse>.Ok(response);
    }

    private static Result<SegmentGlobalizationRequestResponse> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<SegmentGlobalizationRequestResponse>.Fail(
            new Error(code, message, type));
}
