using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchAdvertisements.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchAdvertisements.Query.GetBranchAdvertisements;

internal sealed class GetBranchAdvertisementsQueryHandler
    : IQueryHandler<GetBranchAdvertisementsQuery, IReadOnlyList<BranchAdvertisementResponse>>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<BranchAdvertisement> _advertisementReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetBranchAdvertisementsQueryHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<BranchAdvertisement> advertisementReadRepository,
        ICurrentUser currentUser)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _advertisementReadRepository = advertisementReadRepository
            ?? throw new ArgumentNullException(nameof(advertisementReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<IReadOnlyList<BranchAdvertisementResponse>>> Handle(
        GetBranchAdvertisementsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                new Error(
                    "BranchAdvertisements.Get.Unauthenticated",
                    ErrorMessage.Branch_Authentication_Required,
                    ErrorType.Unauthorized));
        }

        var branchExists = await _branchReadRepository.AnyAsync(
            x => x.Id == request.BranchId,
            cancellationToken);

        if (!branchExists)
        {
            return Result<IReadOnlyList<BranchAdvertisementResponse>>.Fail(
                new Error(
                    "BranchAdvertisements.BranchNotFound",
                    ErrorMessage.Branch_NotFound,
                    ErrorType.NotFound));
        }

        var advertisements = await _advertisementReadRepository.ListAsync(
            new GetBranchAdvertisementsSpec(
                request.BranchId,
                request.IsActive),
            cancellationToken);

        return Result<IReadOnlyList<BranchAdvertisementResponse>>.Ok(
            advertisements);
    }
}
