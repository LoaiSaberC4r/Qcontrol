using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Displays.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Command.RestoreDisplay;

internal sealed class RestoreDisplayCommandHandler
    : ICommandHandler<RestoreDisplayCommand, RestoreDisplayResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteRepository<Display> _displayWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RestoreDisplayCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Display> displayReadRepository,
        IWriteRepository<Display> displayWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));

        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));

        _displayWriteRepository = displayWriteRepository
            ?? throw new ArgumentNullException(nameof(displayWriteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<RestoreDisplayResponse>> Handle(
        RestoreDisplayCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<RestoreDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.Restore.Unauthenticated",
                    Message: ErrorMessage.Display_Authentication_Required,
                    Type: ErrorType.Security));
        }

        var display =
            await _displayReadRepository.FirstOrDefaultAsync(
                new GetDisplayIncludingDeletedSpec(request.Id),
                cancellationToken);

        if (display is null)
        {
            return DisplayNotFound();
        }

        if (!display.IsDeleted)
        {
            return Result<RestoreDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.Restore.AlreadyActive",
                    Message: ErrorMessage.Display_AlreadyActive,
                    Type: ErrorType.Conflict));
        }

        var branch =
            await _branchReadRepository.GetByIdAsync(
                display.BranchId,
                cancellationToken);

        if (branch is null)
        {
            return Result<RestoreDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.Restore.BranchNotFound",
                    Message: ErrorMessage.Display_Branch_NotFound,
                    Type: ErrorType.NotFound));
        }

        var duplicate =
            await CheckRestoreDuplicatesAsync(
                display,
                cancellationToken);

        if (duplicate.IsFailure)
        {
            return duplicate;
        }

        display.Restore();
        _displayWriteRepository.Update(display);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (DisplayUniqueConstraintErrorMapper.TryMapRestore(
                ex,
                out var error))
        {
            return Result<RestoreDisplayResponse>.Fail(error);
        }

        return Result<RestoreDisplayResponse>.Ok(
            new RestoreDisplayResponse
            {
                Id = display.Id,
                BranchId = display.BranchId,
                Number = display.Number,
                IPAddress = display.IPAddress,
                SerialNo = display.SerialNo,
                Type = display.Type,
                RestoredOnUtc = display.RestoredOnUtc,
                Message = ErrorMessage.Display_Restore_Success
            });
    }

    private async Task<Result<RestoreDisplayResponse>> CheckRestoreDuplicatesAsync(
        Display display,
        CancellationToken cancellationToken)
    {
        var existingNumberDisplayId =
            await _displayReadRepository.FirstOrDefaultAsync(
                new DisplayNumberExistsInBranchSpec(
                    display.BranchId,
                    display.Number,
                    excludedDisplayId: display.Id),
                cancellationToken);

        if (existingNumberDisplayId > 0)
        {
            return Result<RestoreDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.Restore.NumberConflict",
                    Message:
                        ErrorMessage
                            .Display_Number_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
        }

        var existingIPAddressDisplayId =
            await _displayReadRepository.FirstOrDefaultAsync(
                new DisplayIPAddressExistsInBranchSpec(
                    display.BranchId,
                    display.IPAddress,
                    excludedDisplayId: display.Id),
                cancellationToken);

        if (existingIPAddressDisplayId > 0)
        {
            return Result<RestoreDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.Restore.IPAddressConflict",
                    Message:
                        ErrorMessage
                            .Display_IPAddress_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
        }

        var existingSerialNoDisplayId =
            await _displayReadRepository.FirstOrDefaultAsync(
                new DisplaySerialNoExistsInBranchSpec(
                    display.BranchId,
                    display.SerialNo,
                    excludedDisplayId: display.Id),
                cancellationToken);

        if (existingSerialNoDisplayId > 0)
        {
            return Result<RestoreDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.Restore.SerialNoConflict",
                    Message:
                        ErrorMessage
                            .Display_SerialNo_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
        }

        return Result<RestoreDisplayResponse>.Ok(null!);
    }

    private static Result<RestoreDisplayResponse> DisplayNotFound()
        => Result<RestoreDisplayResponse>.Fail(
            new Error(
                Code: "Displays.Restore.DisplayNotFound",
                Message: ErrorMessage.Display_NotFound,
                Type: ErrorType.NotFound));
}
