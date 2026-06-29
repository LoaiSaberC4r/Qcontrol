using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Branches.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Command.CreateBranch;

internal sealed class CreateBranchCommandHandler
    : ICommandHandler<CreateBranchCommand, CreateBranchResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<Branch> _branchWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteRepository<Branch> branchWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _branchWriteRepository = branchWriteRepository
            ?? throw new ArgumentNullException(nameof(branchWriteRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<CreateBranchResponse>> Handle(
        CreateBranchCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<CreateBranchResponse>.Fail(new Error(
                Code: "Branches.Create.Unauthenticated",
                Message: ErrorMessage.Branch_Authentication_Required,
                Type: ErrorType.Unauthorized));
        }

        var normalizedIPAddress = IPAddressNormalizer.TryNormalize(
            request.IPAddress,
            out var canonicalIPAddress)
            ? canonicalIPAddress
            : request.IPAddress.Trim();

        var ipAddressAlreadyExists =
            await _branchReadRepository.AnyAsync(
                x => x.IPAddress == normalizedIPAddress,
                cancellationToken);

        if (ipAddressAlreadyExists)
        {
            return Result<CreateBranchResponse>.Fail(new Error(
                Code: "Branches.Create.IPAddressAlreadyExists",
                Message: ErrorMessage.Branch_IPAddress_AlreadyExists,
                Type: ErrorType.Conflict));
        }

        var branch = Branch.Create(
            arabicName: request.ArabicName,
            englishName: request.EnglishName,
            ipAddress: normalizedIPAddress,
            license: request.License,
            governorate: request.Governorate,
            city: request.City,
            area: request.Area,
            address: request.Address,
            latitude: request.Latitude,
            longitude: request.Longitude,
            createdByApplicationUserId: _currentUser.UserId.Value);

        await _branchWriteRepository.AddAsync(
            branch,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var response = new CreateBranchResponse
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
            CreatedByApplicationUserId = branch.CreatedByApplicationUserId,
            CreatedOnUtc = branch.CreatedOnUtc
        };

        return Result<CreateBranchResponse>.Ok(response);
    }
}
