using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Command.DeleteBranch;

internal sealed class DeleteBranchCommandHandler
    : ICommandHandler<DeleteBranchCommand, DeleteBranchResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<Branch> _branchWriteRepository;
    private readonly IWriteRepository<Location> _locationWriteRepository;
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBranchCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteRepository<Branch> branchWriteRepository,
        IWriteRepository<Location> locationWriteRepository,
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        IWriteReadRepository<Display> displayReadRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));

        _branchWriteRepository = branchWriteRepository
            ?? throw new ArgumentNullException(nameof(branchWriteRepository));

        _locationWriteRepository = locationWriteRepository
            ?? throw new ArgumentNullException(nameof(locationWriteRepository));

        _waitingAreaReadRepository = waitingAreaReadRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaReadRepository));

        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<DeleteBranchResponse>> Handle(
        DeleteBranchCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<DeleteBranchResponse>.Fail(new Error(
                Code: "Branches.Delete.Unauthenticated",
                Message: ErrorMessage.Branch_Authentication_Required,
                Type: ErrorType.Security));
        }

        var branch = await _branchReadRepository.FirstOrDefaultAsync(
            new GetBranchForDeleteSpec(request.BranchId),
            cancellationToken);

        if (branch is null)
        {
            return Result<DeleteBranchResponse>.Fail(new Error(
                Code: "Branches.Delete.BranchNotFound",
                Message: ErrorMessage.Branch_NotFound,
                Type: ErrorType.NotFound));
        }

        var hasWaitingAreas = await _waitingAreaReadRepository.AnyAsync(
            x => x.BranchId == branch.Id,
            cancellationToken);

        var displayId = await _displayReadRepository.FirstOrDefaultAsync(
            new AnyDisplayForBranchIncludingDeletedSpec(branch.Id),
            cancellationToken);

        var hasDisplays = displayId > 0;

        if (hasWaitingAreas || hasDisplays)
        {
            return Result<DeleteBranchResponse>.Fail(new Error(
                Code: "Branches.Delete.HasRelatedData",
                Message: ErrorMessage.Branch_Delete_HasRelatedData,
                Type: ErrorType.Conflict));
        }

        if (branch.Location is not null)
        {
            _locationWriteRepository.Delete(branch.Location);
        }

        _branchWriteRepository.Delete(branch);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<DeleteBranchResponse>.Ok(
            new DeleteBranchResponse
            {
                BranchId = branch.Id
            });
    }
}
