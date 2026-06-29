using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Command.DeactivateBranch;

internal sealed class DeactivateBranchCommandHandler
    : ICommandHandler<DeactivateBranchCommand, DeactivateBranchResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<Branch> _branchWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateBranchCommandHandler(
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

    public async Task<Result<DeactivateBranchResponse>> Handle(
        DeactivateBranchCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<DeactivateBranchResponse>.Fail(new Error(
                "Branches.Deactivate.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<DeactivateBranchResponse>.Fail(new Error(
                "Branches.Deactivate.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var branch = await _branchReadRepository.GetByIdTrackedAsync(
            request.BranchId,
            cancellationToken);

        if (branch is null)
        {
            return Result<DeactivateBranchResponse>.Fail(new Error(
                "Branches.Deactivate.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound));
        }

        if (!branch.IsActive)
        {
            return Result<DeactivateBranchResponse>.Fail(new Error(
                "Branches.Deactivate.AlreadyInactive",
                ErrorMessage.Branch_AlreadyInactive,
                ErrorType.Conflict));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(branch, rowVersion);

        branch.Deactivate(
            _dateTimeProvider.UtcNow,
            _currentUser.UserId.Value);

        _branchWriteRepository.Update(branch);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<DeactivateBranchResponse>.Fail(new Error(
                "Branches.Deactivate.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<DeactivateBranchResponse>.Ok(
            new DeactivateBranchResponse
            {
                BranchId = branch.Id,
                IsActive = branch.IsActive,
                EffectiveIsActive = branch.IsActive,
                RowVersion = RowVersionConverter.ToBase64(branch.RowVersion),
                Message = ErrorMessage.Branch_Deactivate_Success
            });
    }
}
