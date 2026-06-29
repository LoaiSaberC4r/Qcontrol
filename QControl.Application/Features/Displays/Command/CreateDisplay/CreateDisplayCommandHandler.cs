using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Displays.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Command.CreateDisplay;

internal sealed class CreateDisplayCommandHandler
    : ICommandHandler<CreateDisplayCommand, CreateDisplayResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteRepository<Display> _displayWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDisplayCommandHandler(
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

    public async Task<Result<CreateDisplayResponse>> Handle(
        CreateDisplayCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<CreateDisplayResponse>.Fail(new Error(
                "Displays.Create.Unauthenticated",
                ErrorMessage.Display_Authentication_Required,
                ErrorType.Unauthorized));
        }

        var normalizedNumber = request.Number.Trim();
        var normalizedIPAddress = IPAddressNormalizer.TryNormalize(
            request.IPAddress,
            out var canonicalIPAddress)
            ? canonicalIPAddress
            : request.IPAddress.Trim();
        var normalizedSerialNo = request.SerialNo.Trim();
        var normalizedType = request.Type.Trim();

        var branch =
            await _branchReadRepository.GetByIdAsync(
                request.BranchId,
                cancellationToken);

        if (branch is null)
        {
            return Result<CreateDisplayResponse>.Fail(new Error(
                "Displays.Create.BranchNotFound",
                ErrorMessage.Display_Branch_NotFound,
                ErrorType.NotFound));
        }

        var duplicate = await CheckCreateDuplicatesAsync(
            request.BranchId,
            normalizedNumber,
            normalizedIPAddress,
            normalizedSerialNo,
            cancellationToken);

        if (duplicate.IsFailure)
        {
            return duplicate;
        }

        var display = Display.Create(
            branchId: request.BranchId,
            number: normalizedNumber,
            ipAddress: normalizedIPAddress,
            serialNo: normalizedSerialNo,
            type: normalizedType,
            createdByApplicationUserId: _currentUser.UserId.Value);

        await _displayWriteRepository.AddAsync(
            display,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (DisplayUniqueConstraintErrorMapper.TryMapCreate(
                ex,
                out var error))
        {
            return Result<CreateDisplayResponse>.Fail(error);
        }

        return Result<CreateDisplayResponse>.Ok(
            new CreateDisplayResponse
            {
                Id = display.Id,
                BranchId = display.BranchId,
                Number = display.Number,
                IPAddress = display.IPAddress,
                SerialNo = display.SerialNo,
                Type = display.Type,
                IsActive = display.IsActive,
                EffectiveIsActive = branch.IsActive && display.IsActive,
                RowVersion = RowVersionConverter.ToBase64(display.RowVersion),
                CreatedByApplicationUserId =
                    display.CreatedByApplicationUserId,
                CreatedOnUtc = display.CreatedOnUtc,
                Message = ErrorMessage.Display_Create_Success
            });
    }

    private async Task<Result<CreateDisplayResponse>> CheckCreateDuplicatesAsync(
        int branchId,
        string number,
        string ipAddress,
        string serialNo,
        CancellationToken cancellationToken)
    {
        var existingNumberDisplayId =
            await _displayReadRepository.FirstOrDefaultAsync(
                new DisplayNumberExistsInBranchSpec(branchId, number),
                cancellationToken);

        if (existingNumberDisplayId > 0)
        {
            return Result<CreateDisplayResponse>.Fail(new Error(
                "Displays.Create.NumberAlreadyExistsInBranch",
                ErrorMessage.Display_Number_AlreadyExistsInBranch,
                ErrorType.Conflict));
        }

        var existingIPAddressDisplayId =
            await _displayReadRepository.FirstOrDefaultAsync(
                new DisplayIPAddressExistsInBranchSpec(branchId, ipAddress),
                cancellationToken);

        if (existingIPAddressDisplayId > 0)
        {
            return Result<CreateDisplayResponse>.Fail(new Error(
                "Displays.Create.IPAddressAlreadyExistsInBranch",
                ErrorMessage.Display_IPAddress_AlreadyExistsInBranch,
                ErrorType.Conflict));
        }

        var existingSerialNoDisplayId =
            await _displayReadRepository.FirstOrDefaultAsync(
                new DisplaySerialNoExistsInBranchSpec(branchId, serialNo),
                cancellationToken);

        if (existingSerialNoDisplayId > 0)
        {
            return Result<CreateDisplayResponse>.Fail(new Error(
                "Displays.Create.SerialNoAlreadyExistsInBranch",
                ErrorMessage.Display_SerialNo_AlreadyExistsInBranch,
                ErrorType.Conflict));
        }

        return Result<CreateDisplayResponse>.Ok(null!);
    }
}
