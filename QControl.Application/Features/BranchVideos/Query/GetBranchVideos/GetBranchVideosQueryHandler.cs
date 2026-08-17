using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.BranchVideos.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchVideos.Query.GetBranchVideos;

internal sealed class GetBranchVideosQueryHandler
    : IQueryHandler<GetBranchVideosQuery, IReadOnlyList<BranchVideoResponse>>
{
    private readonly IWriteReadRepository<Branch> _branchRepository;
    private readonly IWriteReadRepository<BranchVideo> _videoRepository;
    private readonly ICurrentUser _currentUser;

    public GetBranchVideosQueryHandler(
        IWriteReadRepository<Branch> branchRepository,
        IWriteReadRepository<BranchVideo> videoRepository,
        ICurrentUser currentUser)
    {
        _branchRepository = branchRepository;
        _videoRepository = videoRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<BranchVideoResponse>>> Handle(
        GetBranchVideosQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<BranchVideoResponse>>.Fail(new Error(
                "BranchVideos.Get.Unauthenticated",
                ErrorMessage.Branch_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!await _branchRepository.AnyAsync(
                x => x.Id == request.BranchId,
                cancellationToken))
        {
            return Result<IReadOnlyList<BranchVideoResponse>>.Fail(new Error(
                "BranchVideos.BranchNotFound",
                ErrorMessage.Branch_NotFound,
                ErrorType.NotFound));
        }

        var videos = await _videoRepository.ListAsync(
            new GetBranchVideosSpec(request.BranchId, request.IsActive),
            cancellationToken);
        return Result<IReadOnlyList<BranchVideoResponse>>.Ok(videos);
    }
}
