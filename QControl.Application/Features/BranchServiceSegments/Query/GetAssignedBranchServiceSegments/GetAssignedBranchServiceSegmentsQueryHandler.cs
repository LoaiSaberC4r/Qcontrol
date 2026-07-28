using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchServiceSegments.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchServiceSegments.Query.GetAssignedBranchServiceSegments;

internal sealed class GetAssignedBranchServiceSegmentsQueryHandler
    : IQueryHandler<
        GetAssignedBranchServiceSegmentsQuery,
        AssignedBranchServiceSegmentsResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<Service> _services;
    private readonly IWriteReadRepository<BranchService> _branchServices;
    private readonly IWriteReadRepository<BranchServiceSegment> _assignments;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _branchContext;

    public GetAssignedBranchServiceSegmentsQueryHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<Service> services,
        IWriteReadRepository<BranchService> branchServices,
        IWriteReadRepository<BranchServiceSegment> assignments,
        ICurrentUser currentUser,
        ICurrentBranchContext branchContext)
    {
        _branches = branches;
        _services = services;
        _branchServices = branchServices;
        _assignments = assignments;
        _currentUser = currentUser;
        _branchContext = branchContext;
    }

    public async Task<Result<AssignedBranchServiceSegmentsResponse>> Handle(
        GetAssignedBranchServiceSegmentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<AssignedBranchServiceSegmentsResponse>.Fail(
                new Error(
                    "BranchServiceSegments.AuthenticationRequired",
                    BranchServiceSegmentMessages.AuthenticationRequired,
                    ErrorType.Unauthorized));
        }

        var context = await BranchServiceSegmentContextResolver.ResolveAsync(
            request.BranchId,
            request.LeafServiceId,
            _branches,
            _services,
            _branchServices,
            _branchContext,
            cancellationToken);
        if (context.IsFailure)
        {
            return Result<AssignedBranchServiceSegmentsResponse>.Fail(
                context.Errors);
        }

        return Result<AssignedBranchServiceSegmentsResponse>.Ok(
            await BranchServiceSegmentResponseLoader.LoadAsync(
                context.Value,
                _assignments,
                cancellationToken));
    }
}
