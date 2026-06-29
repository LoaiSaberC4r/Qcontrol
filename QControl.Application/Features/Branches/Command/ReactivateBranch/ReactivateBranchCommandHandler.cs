using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Command.ReactivateBranch;

internal sealed class ReactivateBranchCommandHandler
    : ICommandHandler<ReactivateBranchCommand, ReactivateBranchResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<Branch> _branchWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateBranchCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteRepository<Branch> branchWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _branchWriteRepository = branchWriteRepository
            ?? throw new ArgumentNullException(nameof(branchWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ReactivateBranchResponse>> Handle(
        ReactivateBranchCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ReactivateBranchResponse>.Fail(new Error(
                "Branches.Reactivate.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<ReactivateBranchResponse>.Fail(new Error(
                "Branches.Reactivate.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var branch = await _branchReadRepository.GetByIdTrackedAsync(
            request.BranchId,
            cancellationToken);

        if (branch is null)
        {
            return Result<ReactivateBranchResponse>.Fail(new Error(
                "Branches.Reactivate.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound));
        }

        if (branch.IsActive)
        {
            return Result<ReactivateBranchResponse>.Fail(new Error(
                "Branches.Reactivate.AlreadyActive",
                ErrorMessage.Branch_AlreadyActive,
                ErrorType.Conflict));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(branch, rowVersion);

        branch.Reactivate(
            _dateTimeProvider.UtcNow,
            _currentUser.UserId.Value);

        _branchWriteRepository.Update(branch);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ReactivateBranchResponse>.Fail(new Error(
                "Branches.Reactivate.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<ReactivateBranchResponse>.Ok(
            new ReactivateBranchResponse
            {
                BranchId = branch.Id,
                IsActive = branch.IsActive,
                EffectiveIsActive = branch.IsActive,
                RowVersion = RowVersionConverter.ToBase64(branch.RowVersion),
                Message = ErrorMessage.Branch_Reactivate_Success
            });
    }
}
