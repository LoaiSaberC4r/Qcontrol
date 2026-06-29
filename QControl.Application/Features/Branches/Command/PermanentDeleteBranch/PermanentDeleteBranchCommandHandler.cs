using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Command.PermanentDeleteBranch;

internal sealed class PermanentDeleteBranchCommandHandler
    : ICommandHandler<PermanentDeleteBranchCommand, PermanentDeleteBranchResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<Branch> _branchWriteRepository;
    private readonly IWriteRepository<Location> _locationWriteRepository;
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public PermanentDeleteBranchCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteRepository<Branch> branchWriteRepository,
        IWriteRepository<Location> locationWriteRepository,
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        IWriteReadRepository<Display> displayReadRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
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
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<PermanentDeleteBranchResponse>> Handle(
        PermanentDeleteBranchCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var branch = await _branchReadRepository.FirstOrDefaultAsync(
            new GetBranchForDeleteSpec(request.BranchId),
            cancellationToken);

        if (branch is null)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound));
        }

        if (branch.IsActive)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.MustBeInactive",
                ErrorMessage.PermanentDelete_RequiresInactive,
                ErrorType.Conflict));
        }

        var hasWaitingAreas = await _waitingAreaReadRepository.AnyAsync(
            x => x.BranchId == branch.Id,
            cancellationToken);

        var hasDisplays = await _displayReadRepository.AnyAsync(
            x => x.BranchId == branch.Id,
            cancellationToken);

        if (hasWaitingAreas || hasDisplays)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.HasRelatedData",
                ErrorMessage.Branch_PermanentDelete_HasRelatedData,
                ErrorType.Conflict));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(branch, rowVersion);

        if (branch.Location is not null)
        {
            _locationWriteRepository.Delete(branch.Location);
        }

        _branchWriteRepository.Delete(branch);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }
        catch (DbUpdateException)
        {
            return Result<PermanentDeleteBranchResponse>.Fail(new Error(
                "Branches.PermanentDelete.HasRelatedData",
                ErrorMessage.Branch_PermanentDelete_HasRelatedData,
                ErrorType.Conflict));
        }

        return Result<PermanentDeleteBranchResponse>.Ok(
            new PermanentDeleteBranchResponse
            {
                BranchId = request.BranchId,
                Message = ErrorMessage.Branch_PermanentDelete_Success
            });
    }
}
