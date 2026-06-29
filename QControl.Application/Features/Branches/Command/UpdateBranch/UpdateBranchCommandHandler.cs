using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Branches.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Command.UpdateBranch;

internal sealed class UpdateBranchCommandHandler
    : ICommandHandler<UpdateBranchCommand, UpdateBranchResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<Branch> _branchWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBranchCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteRepository<Branch> branchWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
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
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<UpdateBranchResponse>> Handle(
        UpdateBranchCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<UpdateBranchResponse>.Fail(new Error(
                Code: "Branches.Update.Unauthenticated",
                Message: ErrorMessage.Branch_Authentication_Required,
                Type: ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(
                request.RowVersion,
                out var rowVersion))
        {
            return Result<UpdateBranchResponse>.Fail(new Error(
                Code: "Branches.Update.InvalidRowVersion",
                Message: ErrorMessage.RowVersion_Invalid,
                Type: ErrorType.Validation));
        }

        var branch =
            await _branchReadRepository.FirstOrDefaultAsync(
                new GetBranchForUpdateSpec(request.BranchId),
                cancellationToken);

        if (branch is null)
        {
            return Result<UpdateBranchResponse>.Fail(new Error(
                Code: "Branches.Update.BranchNotFound",
                Message: ErrorMessage.Branch_NotFound,
                Type: ErrorType.NotFound));
        }

        var normalizedIPAddress = IPAddressNormalizer.TryNormalize(
            request.IPAddress,
            out var canonicalIPAddress)
            ? canonicalIPAddress
            : request.IPAddress.Trim();

        var ipAddressAlreadyExists =
            await _branchReadRepository.AnyAsync(
                x =>
                    x.Id != request.BranchId &&
                    x.IPAddress == normalizedIPAddress,
                cancellationToken);

        if (ipAddressAlreadyExists)
        {
            return Result<UpdateBranchResponse>.Fail(new Error(
                Code: "Branches.Update.IPAddressAlreadyExists",
                Message: ErrorMessage.Branch_IPAddress_AlreadyExists,
                Type: ErrorType.Conflict));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(
            branch,
            rowVersion);

        branch.Update(
            arabicName: request.ArabicName,
            englishName: request.EnglishName,
            ipAddress: normalizedIPAddress,
            license: request.License,
            lastModifiedByApplicationUserId: _currentUser.UserId.Value);

        branch.UpdateLocation(
            governorate: request.Governorate,
            city: request.City,
            area: request.Area,
            address: request.Address,
            latitude: request.Latitude,
            longitude: request.Longitude);

        _branchWriteRepository.Update(branch);

        try
        {
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<UpdateBranchResponse>.Fail(new Error(
                Code: "Branches.Update.ConcurrencyConflict",
                Message: ErrorMessage.Concurrency_Conflict,
                Type: ErrorType.Conflict));
        }

        var response = new UpdateBranchResponse
        {
            BranchId = branch.Id,
            ArabicName = branch.ArabicName,
            EnglishName = branch.EnglishName,
            IPAddress = branch.IPAddress,
            License = branch.License,
            IsActive = branch.IsActive,
            EffectiveIsActive = branch.IsActive,
            RowVersion = RowVersionConverter.ToBase64(branch.RowVersion),
            Location = new BranchLocationResponse
            {
                Id = branch.Location.Id,
                Governorate = branch.Location.Governorate,
                City = branch.Location.City,
                Area = branch.Location.Area,
                Address = branch.Location.Address,
                Latitude = branch.Location.Latitude,
                Longitude = branch.Location.Longitude
            },
            LastModifiedByApplicationUserId =
                branch.LastModifiedByApplicationUserId,
            ModifiedOnUtc = branch.ModifiedOnUtc
        };

        return Result<UpdateBranchResponse>.Ok(response);
    }
}
