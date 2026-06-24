using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Query.GetBranchDetails;

internal sealed class GetBranchDetailsQueryHandler
    : IQueryHandler<GetBranchDetailsQuery, GetBranchDetailsResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetBranchDetailsQueryHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        ICurrentUser currentUser)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<GetBranchDetailsResponse>> Handle(
        GetBranchDetailsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<GetBranchDetailsResponse>.Fail(new Error(
                Code: "Branches.Details.Unauthenticated",
                Message: ErrorMessage.Branch_Authentication_Required,
                Type: ErrorType.Security));
        }

        var branch = await _branchReadRepository.FirstOrDefaultAsync(
            new GetBranchDetailsSpec(request.BranchId),
            cancellationToken);

        if (branch is null)
        {
            return Result<GetBranchDetailsResponse>.Fail(new Error(
                Code: "Branches.Details.BranchNotFound",
                Message: ErrorMessage.Branch_NotFound,
                Type: ErrorType.NotFound));
        }

        return Result<GetBranchDetailsResponse>.Ok(branch);
    }
}